using BaharAlqeraat.Domain;
using BaharAlqeraat.Domain.Data.Models;
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
using System.Text;
using System.Threading.Tasks;

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
        public async Task<List<QuranLine>> SearchQuran(string searchValue)
        {
            var readers = await _context.QuranLines.Where(x => x.ContentSimple.Contains(searchValue)).ToListAsync();
            return readers;
        }
        public byte[] GetQuraanPage(int readerId, int quranId, int pageNumber)
        {
            var inputFilePath = System.IO.Path.Combine(_env.WebRootPath, readerId.ToString() + "//" + quranId.ToString() + ".pdf");
            byte[] outputBytes;
            string cacheKey = $"{readerId}_{quranId}_{pageNumber}";
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
                        using (PdfWriter writer = new PdfWriter(outputStream, new WriterProperties().SetCompressionLevel(CompressionConstants.BEST_COMPRESSION)))
                        {
                            using (PdfDocument inputDocument = new PdfDocument(reader))
                            {
                                using (PdfDocument outputDocument = new PdfDocument(writer))
                                {
                                    PdfPage inputPage = inputDocument.GetPage(pageNumber);

                                    PageSize outputPageSize = new PageSize(inputPage.GetCropBox());
                                    PdfPage outputPage = outputDocument.AddNewPage(outputPageSize);
                                    //inputPage.GetPdfObject().Put(PdfName.Filter, PdfName.FlateDecode);

                                    PdfFormXObject pageXObject = inputPage.CopyAsFormXObject(outputDocument);

                                    PdfCanvas outputCanvas = new PdfCanvas(outputPage.GetContentStream(0), outputPage.GetResources(), outputDocument);

                                    outputCanvas.AddXObjectAt(pageXObject, 0, 0);

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
    public partial class CustomImage
    {
        public int Id { get; set; }
        public int? IdObj { get; set; }
        public string Url { get; set; }
        public sbyte? Thumbnail { get; set; }
        public string Type { get; set; }
        public byte[] MyImage { get; set; }
    }
}
