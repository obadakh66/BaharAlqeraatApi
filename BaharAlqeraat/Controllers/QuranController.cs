using Bahar.Domain.Data.Dto;
using Bahar.Service;
using BaharAlqeraat.Domain.Data.Dtos;
using BaharAlqeraat.Domain.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BaharAlqeraat.Controllers
{
  
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class QuranController : ControllerBase
    {

        private readonly ILogger<QuranController> _logger;
        private IQuranService _QuranService;
        private IAudioScrapingService _AudioScrapingService;
        private readonly IWebHostEnvironment _env;
        private readonly AppSettings _appSettings;

        public QuranController(ILogger<QuranController> logger, IQuranService QuranService, 
            IWebHostEnvironment env, IAudioScrapingService AudioScrapingService, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _QuranService = QuranService;
            _env = env;
            _AudioScrapingService = AudioScrapingService;
            _appSettings = appSettings.Value;
        }
        [HttpGet]
        public IActionResult GetAppSettings()
        {
            return Ok(_appSettings);
        }

        [HttpGet("{searchValue}/{pageSize}/{pageNumber}")]
        public async Task<IActionResult> Search(string searchValue,int pageSize,int pageNumber)
        {
            try
            {
                var response = await _QuranService.SearchQuran(searchValue,pageSize,pageNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReader(int id)
        {
            try
            {
                var response = await _QuranService.GetReaderAsync(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet("{id}/{suraNumber?}")]
        public async Task<IActionResult> GetSura(int id, int? suraNumber=null)
        {
            try
            {
                var response = await _QuranService.GetSuraAsync(id,suraNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPageSuras(int id, [FromQuery]List<int> suraNumbers)
        {
            try
            {
                var response = await _QuranService.GetPageSurasAsync(id,suraNumbers);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost("DownloadAudio")]
        public async Task<IActionResult> DownloadAudio([FromForm] AudioDownloadRequest request)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var pathToSave = Path.Combine(_env.WebRootPath, "media", request.QuranId.ToString(), request.Surah.ToString());
                    if (!Directory.Exists(pathToSave))
                    {
                        Directory.CreateDirectory(pathToSave);
                    }

                    var fileName = $"{request.Ayah}.mp3";
                    var filePath = Path.Combine(pathToSave, fileName);
                    var isSaved = true;

                    if (!System.IO.File.Exists(filePath))
                    {
                        System.IO.File.WriteAllBytes(filePath, request.AudioBytes);
                        await _AudioScrapingService.LogSuccessfulFiles(request.Ayah, request.QuranId, request.Surah, isSaved);
                        return Ok($"Downloaded and saved: {fileName}");
                    }
                    isSaved= false;
                    await _AudioScrapingService.LogError(request.Ayah, request.QuranId, request.Surah, $"File already exists: {fileName}");
                    return BadRequest($"File already exists: {fileName}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error downloading or saving file: {ex.Message}");
                }
            }
        }
    

    [HttpGet("{id}")]
        public async Task<IActionResult> GetSurasByPageNumber(int id)
        {
            try
            {
                var response = await _QuranService.GetSurasByPageNumberAsync(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
       
        [HttpGet("{readerId}/{quranId}/{pageNumber}")]
        public async Task<IActionResult> GetPage(int readerId, int quranId, int pageNumber)
        {
            try
            {
                var response =await _QuranService.GetQuraanPageAsync(readerId, quranId, pageNumber);
                return File(response, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        [HttpGet("{readerId}/{quranId}/{pageNumber}")]
        public IActionResult GetPageFull(int readerId, int quranId, int pageNumber)
        {
            try
            {
                var response = _QuranService.GetQuraanPageFull(readerId, quranId, pageNumber);
                return File(response, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet("{readerId}/{quranId}/{pageNumber}")]
        public IActionResult GetPageCropped(int readerId, int quranId, int pageNumber)
        {
            try
            {
                var response = _QuranService.GetQuraanPageCropped(readerId, quranId, pageNumber);
                return File(response, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet("{readerId}/{quranId}/{pageNumber}")]
        public IActionResult GetPageText(int readerId, int quranId, int pageNumber)
        {
            try
            {
                var response = _QuranService.GetQuraanPageText(readerId, quranId, pageNumber);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet]
        public async Task<IActionResult> ListReaders()
        {
            try
            {
                var respoinse = await _QuranService.ListReaders();
                return Ok(respoinse);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
