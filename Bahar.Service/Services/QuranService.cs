using BaharAlqeraat.Domain;
using BaharAlqeraat.Domain.Data.Dtos;
using BaharAlqeraat.Domain.Data.Models;
using HtmlAgilityPack;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Xobject;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Bahar.Service
{
    public class QuranService : IQuranService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _memoryCache;

        public QuranService(ApplicationDbContext context, IWebHostEnvironment env, IMemoryCache memoryCache)
        {
            _context = context;
            _env = env;
            _memoryCache = memoryCache;

        }
        public async Task<List<QuranReader>> ListReaders()
        {
            var readers = await _context.Readers.Include(x => x.Qurans).ToListAsync();
            return readers;
        }
        public async Task<QuranReader> GetReaderAsync(int id)
        {
            var reader = await _context.Readers.Include(x => x.Qurans).FirstOrDefaultAsync(x => x.Id == id);
            return reader;
        }
        public async Task<SuraDto> GetSuraAsync(int pageId)
        {
            var ayat = await _context.QuranLines
                .Where(x => x.PageNumber == pageId)
                .Select(x => new
                {
                    x.SuraName,
                    x.SuraNumber,
                    x.PageNumber,
                    x.Content,
                    x.ContentSimple,
                    x.Id,
                    x.Number
                })
                .Distinct()
                .ToListAsync();

            var firstAyah = ayat.FirstOrDefault();

            if (firstAyah == null)
            {
                // Handle when no data is found for the given pageId
                return null; // Or handle accordingly
            }

            var response = new SuraDto
            {
                SuraName = firstAyah.SuraName,
                PageNumber = pageId,
                SuraNumber = firstAyah.SuraNumber
            };

            response.Ayat = ayat.Select(x => new AyahDto
            {
                Content = x.Content,
                ContentSimple = x.ContentSimple,
                Id = x.Id,
                Number = x.Number,
                PageNumber = x.PageNumber,
                SuraName = x.SuraName,
                SuraNumber = x.SuraNumber
            }).OrderBy(x=>x.Number).ToList();

            return response;
        }
        public async Task ScrapeAndDownloadAudioFilesAsync()
        {
            try
            {
                var url = "https://www.nquran.com/ar/ayacompare/%D9%85%D9%82%D8%A7%D8%B1%D9%86%D8%A9-%D8%A7%D9%84%D8%A2%D9%8A%D8%A7%D8%AA-%D8%A8%D8%A7%D9%84%D8%B1%D9%88%D8%A7%D9%8A%D8%A7%D8%AA?&aya=2&sora=1";

                await new BrowserFetcher().DownloadAsync();
                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using var page = await browser.NewPageAsync();

                await page.GoToAsync(url);

                // Wait for the page to load and any potential dynamic content to be rendered
                await page.WaitForTimeoutAsync(5000);

                // Click the play button
                await page.ClickAsync(".fa-headphones");

                // Wait for the audio to play (you may need to adjust this duration)
                await page.WaitForTimeoutAsync(5000);

                // Extract the URL of the audio being played
                var audioUrl = await page.EvaluateFunctionAsync<string>(
                    @"() => document.querySelector('audio').src"
                );

                if (!string.IsNullOrEmpty(audioUrl))
                {
                    var httpClient = new System.Net.Http.HttpClient();
                    var audioBytes = await httpClient.GetByteArrayAsync(audioUrl);

                    // Save the audio file
                    var fileName = System.IO.Path.GetFileName(audioUrl);
                    File.WriteAllBytes(fileName, audioBytes);
                    Console.WriteLine($"Downloaded: {fileName}");
                }
                else
                {
                    Console.WriteLine("Failed to extract audio URL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public async Task<QuranLineResponse> SearchQuran(string searchValue, int pageSize, int pageNumber)
        {
            var readers = _context.QuranLines.Where(x => x.ContentSimple.Contains(searchValue));
            QuranLineResponse response = new QuranLineResponse
            {
                TotalCount = readers.Count(),
                Result = await readers.Skip(pageSize * (pageNumber - 1))
                .Take(pageSize).ToListAsync()
            };
            return response;
        }

        public async Task<byte[]> GetQuraanPageAsync(int readerId, int quranId, int pageNumber)
        {
            string inputFilePath = string.Format(System.IO.Path.Combine(_env.WebRootPath, "{0}//{1}.pdf"), readerId, quranId);

            byte[] outputBytes;
            string cacheKey = $"{readerId}_{quranId}_{pageNumber}";

            if (_memoryCache.TryGetValue(cacheKey, out outputBytes))
            {
                return outputBytes;
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                using (PdfReader reader = new PdfReader(inputFilePath))
                {
                    using (PdfWriter writer = new PdfWriter(outputStream))
                    {
                        using (PdfDocument inputDocument = new PdfDocument(reader))
                        {
                            using (PdfDocument outputDocument = new PdfDocument(writer))
                            {
                                PdfPage inputPage = inputDocument.GetPage(pageNumber);

                                PageSize outputPageSize = new PageSize(inputPage.GetCropBox());
                                PdfPage outputPage = outputDocument.AddNewPage(outputPageSize);

                                PdfFormXObject pageXObject = inputPage.CopyAsFormXObject(outputDocument);

                                PdfCanvas outputCanvas = new PdfCanvas(outputPage.GetContentStream(0), outputPage.GetResources(), outputDocument);
                                outputCanvas.AddXObjectAt(pageXObject, 0, 0);
                            }
                        }
                    }
                }

                outputBytes = outputStream.ToArray();
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
            _memoryCache.Set(cacheKey, outputBytes, cacheEntryOptions);

            return outputBytes;
        }
        //public byte[] GetPdfPageImgeAsync(int readerId, int quranId, int pageNumber)
        //{
        //    var inputFilePath = System.IO.Path.Combine(_env.WebRootPath, readerId.ToString() + "//" + quranId.ToString() + ".pdf");

        //    // Check if the file is already in cache
        //    byte[] pdfFileBytes = File.ReadAllBytes(inputFilePath);
        //    byte[] imageBytes;
        //    // Create a MemoryStream from the byte array
        //    string cacheKey = $"{readerId}_{quranId}";
        //    if (_memoryCache.TryGetValue(cacheKey, out byte[] pdfBytes))
        //    {
        //        return pdfBytes;
        //    }
        //    else
        //    {
        //        using (MemoryStream pdfStream = new MemoryStream(pdfFileBytes))
        //        {
        //            // Set up the Ghostscript rasterizer
        //            GhostscriptRasterizer rasterizer = new GhostscriptRasterizer();

        //            // Set the resolution for the output image (dpi)
        //            int dpi = 400;

        //            // Convert the first page of the PDF to a JPEG image
        //            using (MemoryStream imageStream = new MemoryStream())
        //            {
        //                rasterizer.Open(pdfStream);
        //                var pdfPage = rasterizer.GetPage(dpi, pageNumber);
        //                int width = pdfPage.Width;
        //                int height = pdfPage.Height;


        //                pdfPage.Save(imageStream, ImageFormat.Jpeg);
        //                rasterizer.Close();

        //                // Get the byte array of the JPEG image
        //                imageBytes = imageStream.ToArray();
        //                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
        //                _memoryCache.Set(cacheKey, imageBytes, cacheEntryOptions);
        //            }
        //            return imageBytes;

        //        }
        //    }

        //}

        public byte[] GetQuraanPageFull(int readerId, int quranId, int pageNumber)
        {
            var inputFilePath = System.IO.Path.Combine(_env.WebRootPath, readerId.ToString() + "//" + quranId.ToString() + ".pdf");
            byte[] outputBytes;
            string cacheKey = $"{readerId}_{quranId}_{pageNumber}_full";
            if (_memoryCache.TryGetValue(cacheKey, out byte[] pdfBytes))
            {
                return pdfBytes;
            }
            else
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (PdfReader reader = new PdfReader(inputFilePath))
                    {
                        using (PdfWriter writer = new PdfWriter(outputStream))
                        {
                            using (PdfDocument inputDocument = new PdfDocument(reader))
                            {
                                using (PdfDocument outputDocument = new PdfDocument(writer))
                                {
                                    PdfPage inputPage = inputDocument.GetPage(pageNumber);

                                    // Set the output page height to be the same as the input page height
                                    float outputPageHeight = inputPage.GetCropBox().GetHeight();

                                    // Calculate the output page width to maintain the aspect ratio and adjust it by the 85% factor
                                    float outputPageWidth = inputPage.GetCropBox().GetWidth() * (outputPageHeight / inputPage.GetCropBox().GetHeight()) * 0.85f;
                                    PageSize outputPageSize = new PageSize(outputPageWidth, outputPageHeight);

                                    PdfPage outputPage = outputDocument.AddNewPage(outputPageSize);

                                    // Add the XObject to the output page
                                    PdfFormXObject pageXObject = inputPage.CopyAsFormXObject(outputDocument);
                                    PdfCanvas outputCanvas = new PdfCanvas(outputPage);

                                    outputCanvas.AddXObject(pageXObject, 0, 0);

                                    // Crop the empty edges
                                    outputCanvas.Rectangle(outputPageSize.GetLeft(), outputPageSize.GetBottom(),
                                                           outputPageSize.GetWidth(), outputPageSize.GetHeight())
                                              .Clip()
                                              .EndPath();

                                    outputDocument.Close();

                                }
                            }
                        }
                    }

                    outputBytes = outputStream.ToArray();
                }
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _memoryCache.Set(cacheKey, outputBytes, cacheEntryOptions);
            }

            return outputBytes;
        }

        public byte[] GetQuraanPageCropped(int readerId, int quranId, int pageNumber)
        {
            var inputFilePath = System.IO.Path.Combine(_env.WebRootPath, readerId.ToString() + "//" + quranId.ToString() + ".pdf");
            byte[] outputBytes;
            string cacheKey = $"{readerId}_{quranId}_{pageNumber}_full";
            if (_memoryCache.TryGetValue(cacheKey, out byte[] pdfBytes))
            {
                return pdfBytes;
            }
            else
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (PdfReader reader = new PdfReader(inputFilePath))
                    {
                        using (PdfWriter writer = new PdfWriter(outputStream))
                        {
                            using (PdfDocument inputDocument = new PdfDocument(reader))
                            {
                                using (PdfDocument outputDocument = new PdfDocument(writer))
                                {
                                    PdfPage inputPage = inputDocument.GetPage(pageNumber);

                                    // Crop top and bottom margins
                                    float topCropPercentage = 0.04f;
                                    float bottomCropPercentage = 0.20f;
                                    float inputPageHeight = inputPage.GetCropBox().GetHeight();
                                    float croppedPageHeight = inputPageHeight * (1 - topCropPercentage - bottomCropPercentage);
                                    float outputPageHeight = croppedPageHeight;

                                    // Set the output page size
                                    PageSize outputPageSize = new PageSize(inputPage.GetCropBox().GetWidth(), outputPageHeight);
                                    PdfPage outputPage = outputDocument.AddNewPage(outputPageSize);

                                    // Add the XObject to the output page
                                    PdfFormXObject pageXObject = inputPage.CopyAsFormXObject(outputDocument);
                                    PdfCanvas outputCanvas = new PdfCanvas(outputPage);

                                    float topCropHeight = inputPageHeight * topCropPercentage;
                                    outputCanvas.AddXObject(pageXObject, 0, -topCropHeight);

                                    // Crop the empty edges
                                    outputCanvas.Rectangle(outputPageSize.GetLeft(), outputPageSize.GetBottom(),
                                                           outputPageSize.GetWidth(), outputPageSize.GetHeight())
                                              .Clip()
                                              .EndPath();

                                    outputDocument.Close();
                                }
                            }
                        }
                    }

                    outputBytes = outputStream.ToArray();
                }
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _memoryCache.Set(cacheKey, outputBytes, cacheEntryOptions);
            }

            return outputBytes;
        }
        public void ConvertQuraanFull(int readerId, int quranId)
        {
            var inputFilePath = System.IO.Path.Combine(_env.WebRootPath, readerId.ToString() + "//" + quranId.ToString() + ".pdf");
            var outputFilePath = System.IO.Path.Combine(_env.WebRootPath, $"{readerId}_{quranId}_full.pdf");

            byte[] outputBytes;
            string cacheKey = $"{readerId}_{quranId}_full";
            if (_memoryCache.TryGetValue(cacheKey, out byte[] pdfBytes))
            {
                //return pdfBytes;
            }
            else
            {
                using (PdfReader reader = new PdfReader(inputFilePath))
                {
                    using (PdfWriter writer = new PdfWriter(outputFilePath))
                    {
                        using (PdfDocument inputDocument = new PdfDocument(reader))
                        {
                            var lastPageNumber = inputDocument.GetNumberOfPages();
                            using (PdfDocument outputDocument = new PdfDocument(writer))
                            {
                                for (int pageNumber = 1; pageNumber <= lastPageNumber; pageNumber++)
                                {
                                    PdfPage inputPage = inputDocument.GetPage(pageNumber);

                                    // Set the output page height to be the same as the input page height
                                    float outputPageHeight = inputPage.GetCropBox().GetHeight();
                                    PageSize outputPageSize = new PageSize(PageSize.A4.GetWidth(), outputPageHeight);

                                    // Calculate the output page width to maintain the aspect ratio and adjust it by the 85% factor
                                    float outputPageWidth = inputPage.GetCropBox().GetWidth() * (outputPageHeight / inputPage.GetCropBox().GetHeight()) * 0.85f;
                                    outputPageSize = new PageSize(outputPageWidth, outputPageHeight);

                                    PdfPage outputPage = outputDocument.AddNewPage(outputPageSize);

                                    // Add the XObject to the output page
                                    PdfFormXObject pageXObject = inputPage.CopyAsFormXObject(outputDocument);
                                    PdfCanvas outputCanvas = new PdfCanvas(outputPage);

                                    outputCanvas.AddXObject(pageXObject, 0, 0);

                                    // Crop the empty edges
                                    outputCanvas.Rectangle(outputPageSize.GetLeft(), outputPageSize.GetBottom(),
                                                           outputPageSize.GetWidth(), outputPageSize.GetHeight())
                                              .Clip()
                                              .EndPath();

                                }
                                outputDocument.Close();

                            }
                        }
                    }
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _memoryCache.Set(cacheKey, pdfBytes, cacheEntryOptions);
            }

            //return outputBytes;
        }

        public string GetQuraanPageText(int readerId, int quranId, int pageNumber)
        {
            var inputFilePath = System.IO.Path.Combine(_env.WebRootPath, readerId.ToString() + "//" + quranId.ToString() + ".pdf");
            byte[] outputBytes;

            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(inputFilePath)))
            {
                // Get the page object for the given page number
                PdfPage pdfPage = pdfDocument.GetPage(pageNumber);

                // Create a text extraction strategy
                LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();

                // Parse the page content and extract the text
                PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                parser.ProcessPageContent(pdfPage);

                // Convert the extracted text to the desired encoding
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(strategy.GetResultantText());
                string pageText = Encoding.UTF8.GetString(utf8Bytes);

                return pageText;
            }
        }
    }
    public class EmptyPdfPageExtraCopier : IPdfPageExtraCopier
    {
        public void Copy(PdfPage srcPage, PdfPage destPage) { }
    }

}
