using Bahar.Service;
using BaharAlqeraat.Domain;
using BaharAlqeraat.Domain.Data.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class AudioScrapingService : IAudioScrapingService
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AudioScrapingService> _logger;
    public int ayahId = 0;
    public int surahId = 0;
    public int quranId;
    private IBrowser _browser;
    public List<int> GlobalQuranIds = new List<int> { 19, 4, 5, 6, 8, 9, 12, 13, 15, 16, 18, 22, 24, 28, 30, 33, 34, 36, 37, 39, 40 };
    public AudioScrapingService(IWebHostEnvironment env, ApplicationDbContext context, ILogger<AudioScrapingService> logger)
    {
        _env = env;
        _context = context;
        _logger = logger;
    }

    public async Task ScrapeAndDownloadAudioFilesAsync(List<int> quransIds, CancellationToken cancellationToken, int? startingSurah = null, int? startingAyah = null)
    {
        IBrowser browser = null;
        try
        {
            int startSurah;
            int startAyah;

            if (startingSurah.HasValue)
                startSurah = startingSurah.Value;
            else
            {
                var latestLog = await _context.WebScrapeLogs
                            .OrderByDescending(log => log.Id)
                            .FirstOrDefaultAsync();

                startSurah = latestLog?.SurahId ?? 1;
            }

            startAyah = startingAyah ?? 1;

            var quranSurahNumbers = await _context.QuranLines
                .Where(x => x.SuraNumber >= startSurah)
                .Select(x => x.SuraNumber)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            Log.Information("Browser initialized");

            foreach (var surah in quranSurahNumbers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                surahId = surah;

                var surahAyat = await _context.QuranLines
                    .Where(x => x.SuraNumber == surah && x.Number >= startAyah)
                    .Select(x => x.Number)
                    .OrderBy(x => x)
                    .Distinct()
                    .ToListAsync();

                foreach (var ayah in surahAyat)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var ayahId = ayah;

                    foreach (var gQuranId in GlobalQuranIds)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await PerformTaskAsync(gQuranId, surah, ayah, browser, cancellationToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Operation was cancelled.");
        }
        catch (Exception ex)
        {
            Log.Error("ayah :" + ayahId + " surah :" + surahId + " exception : " + ex.Message);
            await LogError(ayahId, quranId, surahId, ex.Message, cancellationToken);
            throw;
        }
        finally
        {
            if (browser != null)
            {
                await browser.CloseAsync();
                Log.Information("Browser closed.");
            }
        }
    
    }
    private async Task PerformTaskAsync(int gQuranId, int surah, int ayah, IBrowser browser, CancellationToken cancellationToken)
    {
        var pathToSave = Path.Combine(_env.WebRootPath, "media", gQuranId.ToString(), surah.ToString());
        if (!Directory.Exists(pathToSave))
        {
            Directory.CreateDirectory(pathToSave);
        }

        var fileName = $"{ayah}.mp3";
        var filePath = Path.Combine(pathToSave, fileName);
        Log.Information("ayah :" + ayah + " surah :" + surah + " ");
        if (!System.IO.File.Exists(filePath))
        {
            var url = $"https://www.nquran.com/ar/ayacompare?aya={ayah}&sora={surah}";

            using var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            await page.WaitForTimeoutAsync(5000);

            var audioElements = await page.QuerySelectorAllAsync(".fa-headphones");
            var audioToSaveList = new List<AyaAduiDto>();

            var quranIndexMap = new Dictionary<int, int>
        {
            { 19, 0 }, { 4, 1 }, { 5, 2 }, { 6, 3 }, { 8, 4 }, { 9, 5 },
            { 12, 6 }, { 13, 7 }, { 15, 8 }, { 16, 9 }, { 18, 10 },
            { 22, 11 }, { 24, 12 }, { 28, 13 }, { 30, 14 }, { 33, 15 },
            { 34, 16 }, { 36, 17 }, { 37, 18 }, { 39, 19 }, { 40, 20 }
        };

            if (quranIndexMap.TryGetValue(gQuranId, out var index))
            {
                audioToSaveList.Add(new AyaAduiDto
                {
                    QuranId = gQuranId,
                    Audio = audioElements[index]
                });
            }

            foreach (var audioElement in audioToSaveList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await audioElement.Audio.ClickAsync();
                var audioPlaying = false;
                var startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalMilliseconds < 30000)
                {
                    audioPlaying = await page.EvaluateFunctionAsync<bool>("() => document.querySelector('audio').paused === false");
                    if (audioPlaying) break;
                    await page.WaitForTimeoutAsync(5000);
                }

                if (audioPlaying)
                {
                    try
                    {
                        await page.WaitForTimeoutAsync(5000);
                        var audioUrl = await page.EvaluateFunctionAsync<string>("() => document.querySelector('audio').src");

                        if (!string.IsNullOrEmpty(audioUrl))
                        {
                            await audioElement.Audio.ClickAsync();
                            var httpClient = new HttpClient();
                            var audioBytes = await httpClient.GetByteArrayAsync(audioUrl);
                            var audioDownloadRequest = new AudioDownloadRequest
                            {
                                AudioBytes = audioBytes,
                                Ayah = ayah,
                                QuranId = audioElement.QuranId,
                                Surah = surah
                            };
                            await CallDownloadApi(audioDownloadRequest, cancellationToken);
                        }
                        else
                        {
                            await LogError(ayah, audioElement.QuranId, surah, "Failed to fetch audio url.", cancellationToken);
                        }
                    }
                    catch (Exception e)
                    {
                        await LogError(ayah, audioElement.QuranId, surah, e.Message, cancellationToken);
                    }
                }
                else
                {
                    await LogError(ayah, audioElement.QuranId, surah, "Failed to start playing audio.", cancellationToken);
                }
            }
        }
    }
    private async Task CallDownloadApi(AudioDownloadRequest request, CancellationToken cancellationToken)
    {
        var pathToSave = Path.Combine(_env.WebRootPath, "media", request.QuranId.ToString(), request.Surah.ToString());
        if (!Directory.Exists(pathToSave))
        {
            Directory.CreateDirectory(pathToSave);
        }

        var fileName = $"{request.Ayah}.mp3";
        var filePath = Path.Combine(pathToSave, fileName);
        Log.Information("ayah :" + request.Ayah + " surah :" + request.Surah + " quran: " + request.QuranId);
        if (!System.IO.File.Exists(filePath))
        {
            System.IO.File.WriteAllBytes(filePath, request.AudioBytes);
            await LogSuccessfulFiles(request.Ayah, request.QuranId, request.Surah, false, cancellationToken);
        }
        else
        {
            await LogSuccessfulFiles(request.Ayah, request.QuranId, request.Surah, true, cancellationToken);
            await LogError(request.Ayah, request.QuranId, request.Surah, $"File already exists: {fileName}", cancellationToken);
        }
    }

    public async Task LogSuccessfulFiles(int ayahId, int quranId, int surahId, bool isSaved, CancellationToken cancellationToken)
    {
        await _context.WebScrapeLogs.AddAsync(new BaharAlqeraat.Domain.Data.Models.WebScrapeLog
        {
            AyahId = ayahId,
            CreationDate = DateTime.Now,
            QuranId = quranId,
            SurahId = surahId,
            IsSavedBefore = isSaved
        }, cancellationToken);
        // Log successful files to another table
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogError(int ayahId, int quranId, int surahId, string error, CancellationToken cancellationToken)
    {
        try
        {
            await _context.ErrorLogs.AddAsync(new BaharAlqeraat.Domain.Data.Models.ErrorLog
            {
                AyahId = ayahId,
                CreationDate = DateTime.Now,
                QuranId = quranId,
                SurahId = surahId,
                Error = error
            }, cancellationToken);
            // Log successful files to another table
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine("error log creation failed ", e.Message);
            throw;
        }
    }
}