using Bahar.Service;
using BaharAlqeraat.Domain;
using BaharAlqeraat.Domain.Data.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class AudioScrapingService : IAudioScrapingService
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _context;
    public int ayahId = 0;
    public int surahId = 0;
    public int quranId;
    private IBrowser _browser;
    public List<int> GlobalQuranIds = new List<int> { 19, 4, 5, 6, 8, 9, 12, 13, 15, 16, 18, 22, 24, 28, 30, 33, 34, 36, 37, 39, 40 };
    public AudioScrapingService(IWebHostEnvironment env, ApplicationDbContext context)
    {
        _env = env;
        _context = context;
    }

    public async Task ScrapeAndDownloadAudioFilesAsync(List<int> quransIds, int? startingSurah = null, int? startingAyah = null)
    {
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
            if (startingAyah.HasValue)
                startAyah = startingAyah.Value;
            else
            {
                startAyah = 1;
            }

            var quranSurahNumbers = await _context.QuranLines
                .Where(x => x.SuraNumber >= startSurah)
                .Select(x => x.SuraNumber)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            foreach (var surah in quranSurahNumbers)
            {
                surahId = surah;

                var surahAyat = await _context.QuranLines
                    .Where(x => x.SuraNumber == surah && x.Number >= startAyah)
                    .Select(x => x.Number)
                    .OrderBy(x => x)
                    .Distinct()
                    .ToListAsync();
                foreach (var ayah in surahAyat)
                {
                    quransIds = new List<int>();
                    foreach (var gQuranId in GlobalQuranIds)
                    {
                        var pathToSave = Path.Combine(_env.WebRootPath, "media", gQuranId.ToString(), surah.ToString());
                        if (!Directory.Exists(pathToSave))
                        {
                            Directory.CreateDirectory(pathToSave);
                        }

                        var fileName = $"{ayah}.mp3";
                        var filePath = Path.Combine(pathToSave, fileName);

                        if (!System.IO.File.Exists(filePath))
                        {
                            if (!quransIds.Contains(gQuranId))
                            {
                                quransIds.Add(gQuranId);
                            }
                        }
                    }
                    ayahId = ayah;
                    var scrabeLog = await _context.WebScrapeLogs.Where(x => x.SurahId == surah && x.AyahId == ayah && x.IsSavedBefore).OrderByDescending(x => x.CreationDate).FirstOrDefaultAsync();
                    if (quransIds.Count > 0)
                    {
                        var url = "https://www.nquran.com/ar/ayacompare?aya=" + ayah + "&sora=" + surah;
                        await new BrowserFetcher().DownloadAsync();

                        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                        using var page = await browser.NewPageAsync();

                        await page.GoToAsync(url);

                        // Wait for the page to load and any potential dynamic content to be rendered
                        await page.WaitForTimeoutAsync(5000);

                        // Get all elements with the class ".fa-headphones"
                        var audioElements = await page.QuerySelectorAllAsync(".fa-headphones");
                        var qurans = await _context.Qurans.ToListAsync();
                        var audioToSaveList = new List<AyaAduiDto>();
                        int index = -1;
                        if (quransIds.Count > 0)
                        {
                            foreach (var id in quransIds)
                            {
                                switch (id)
                                {
                                    case 19:
                                        index = 0;
                                        break;
                                    case 4:
                                        index = 1;
                                        break;
                                    case 5:
                                        index = 2;
                                        break;
                                    case 6:
                                        index = 3;
                                        break;
                                    case 8:
                                        index = 4;
                                        break;
                                    case 9:
                                        index = 5;
                                        break;
                                    case 12:
                                        index = 6;
                                        break;
                                    case 13:
                                        index = 7;
                                        break;
                                    case 15:
                                        index = 8;
                                        break;
                                    case 16:
                                        index = 9;
                                        break;
                                    case 18:
                                        index = 10;
                                        break;
                                    case 22:
                                        index = 11;
                                        break;
                                    case 24:
                                        index = 12;
                                        break;
                                    case 28:
                                        index = 13;
                                        break;
                                    case 30:
                                        index = 14;
                                        break;
                                    case 33:
                                        index = 15;
                                        break;
                                    case 34:
                                        index = 16;
                                        break;
                                    case 36:
                                        index = 17;
                                        break;
                                    case 37:
                                        index = 18;
                                        break;
                                    case 39:
                                        index = 19;
                                        break;
                                    case 40:
                                        index = 20;
                                        break;
                                    default:
                                        // Handle the case where the QuranId is not found
                                        break;
                                }
                                audioToSaveList.Add(new AyaAduiDto
                                {
                                    QuranId = id,
                                    Audio = audioElements[index]
                                });
                            }
                        }
                        else
                        {
                            audioToSaveList = new List<AyaAduiDto>
                                    {
                                        new AyaAduiDto { Audio= audioElements[0], QuranId=19 },
                                        new AyaAduiDto { Audio= audioElements[1], QuranId=4 },
                                        new AyaAduiDto { Audio= audioElements[2], QuranId=5 },
                                        new AyaAduiDto { Audio= audioElements[3], QuranId=6 },
                                        new AyaAduiDto { Audio= audioElements[4], QuranId=8 },
                                        new AyaAduiDto { Audio= audioElements[5], QuranId=9 },
                                        new AyaAduiDto { Audio= audioElements[6], QuranId=12 },
                                        new AyaAduiDto { Audio= audioElements[7], QuranId=13 },
                                        new AyaAduiDto { Audio= audioElements[8], QuranId=15 },
                                        new AyaAduiDto { Audio= audioElements[9], QuranId=16 },
                                        new AyaAduiDto { Audio= audioElements[10], QuranId=18 },
                                        new AyaAduiDto { Audio= audioElements[11], QuranId=22 },
                                        new AyaAduiDto { Audio= audioElements[12], QuranId=24 },
                                        new AyaAduiDto { Audio= audioElements[13], QuranId=28 },
                                        new AyaAduiDto { Audio= audioElements[14], QuranId=30 },
                                        new AyaAduiDto { Audio= audioElements[15], QuranId=33 },
                                        new AyaAduiDto { Audio= audioElements[16], QuranId=34 },
                                        new AyaAduiDto { Audio= audioElements[17], QuranId=36 },
                                        new AyaAduiDto { Audio= audioElements[18], QuranId=37 },
                                        new AyaAduiDto { Audio= audioElements[19], QuranId=39 },
                                        new AyaAduiDto { Audio= audioElements[20], QuranId=40 }

                                    };
                        }

                        // Click each ".fa-headphones" element

                        foreach (var audioelement in audioToSaveList)
                        {
                            await audioelement.Audio.ClickAsync();
                            var startTime = DateTime.Now;
                            var audioPlaying = false;
                            quranId = audioelement.QuranId;
                            while ((DateTime.Now - startTime).TotalMilliseconds < 30000)  // Adjust the maximum waiting time as needed
                            {
                                // Check if the audio is playing by evaluating a condition
                                audioPlaying = await page.EvaluateFunctionAsync<bool>(
                                    @"() => document.querySelector('audio').paused === false"
                                );

                                if (audioPlaying)
                                {
                                    // Audio has started playing, break out of the loop
                                    break;
                                }

                                // Sleep for a short duration before checking again
                                await page.WaitForTimeoutAsync(5000);  // Adjust the interval as needed
                            }
                            if (audioPlaying)
                            {
                                try
                                {
                                    await page.WaitForTimeoutAsync(5000);
                                    // Extract the URL of the audio being played
                                    var audioUrl = await page.EvaluateFunctionAsync<string>(
                                        @"() => document.querySelector('audio').src"
                                    );

                                    if (!string.IsNullOrEmpty(audioUrl))
                                    {

                                        await audioelement.Audio.ClickAsync();
                                        var httpClient = new System.Net.Http.HttpClient();
                                        var audioBytes = await httpClient.GetByteArrayAsync(audioUrl);
                                        AudioDownloadRequest audioDownloadRequest = new AudioDownloadRequest
                                        {
                                            AudioBytes = audioBytes,
                                            Ayah = ayah,
                                            QuranId = audioelement.QuranId,
                                            Surah = surah
                                        };
                                        await CallDownloadApi(audioDownloadRequest);
                                    }

                                    else
                                    {
                                        await LogError(ayah, audioelement.QuranId, surah, "Failed to fetch audio url.");
                                        Console.WriteLine($"Failed to extract audio URL. surah: {surah}, ayah: {ayah}");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Failed to start playing audio. surah: {surah}, ayah: {ayah} because {e.Message}");
                                    await LogError(ayah, audioelement.QuranId, surah, e.Message);
                                }
                            }
                            else
                            {
                                await LogError(ayah, audioelement.QuranId, surah, "Failed to start playing audio.");
                                Console.WriteLine($"Failed to start playing audio. surah: {surah}, ayah: {ayah}");
                            }
                        }
                    }


                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"------------ exception happened -----------------");
            await LogError(ayahId, quranId, surahId, ex.Message);
            throw;
        }
    }
    private async Task CallDownloadApi(AudioDownloadRequest request)
    {

        var pathToSave = Path.Combine(_env.WebRootPath, "media", request.QuranId.ToString(), request.Surah.ToString());
        if (!Directory.Exists(pathToSave))
        {
            Directory.CreateDirectory(pathToSave);
        }

        var fileName = $"{request.Ayah}.mp3";
        var filePath = Path.Combine(pathToSave, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            System.IO.File.WriteAllBytes(filePath, request.AudioBytes);
            await LogSuccessfulFiles(request.Ayah, request.QuranId, request.Surah, false);
        }
        else
        {
            await LogSuccessfulFiles(request.Ayah, request.QuranId, request.Surah, true);
            await LogError(request.Ayah, request.QuranId, request.Surah, $"File already exists: {fileName}");
        }
    }
    public async Task LogSuccessfulFiles(int ayahId, int quranId, int surahId, bool isSaved)
    {
        await _context.WebScrapeLogs.AddAsync(new BaharAlqeraat.Domain.Data.Models.WebScrapeLog
        {
            AyahId = ayahId,
            CreationDate = DateTime.Now,
            QuranId = quranId,
            SurahId = surahId,
            IsSavedBefore = isSaved
        });
        // Log successful files to another table
        await _context.SaveChangesAsync();
    }
    public async Task LogError(int ayahId, int quranId, int surahId, string error)
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
            });
            // Log successful files to another table
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("error log creation failed ", e.Message);
            throw e;
        }
    }
}