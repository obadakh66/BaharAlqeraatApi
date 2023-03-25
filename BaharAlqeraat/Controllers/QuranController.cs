using Bahar.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BaharAlqeraat.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class QuranController : ControllerBase
    {

        private readonly ILogger<QuranController> _logger;
        private IQuranService _QuranService;

        public QuranController(ILogger<QuranController> logger, IQuranService QuranService)
        {
            _logger = logger;
            _QuranService = QuranService;
        }

        [HttpGet("{searchValue}")]
        public async Task<IActionResult> Search(string searchValue)
        {
            try
            {
                var response = await _QuranService.SearchQuran(searchValue);
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
       
        [HttpGet("{readerId}/{quranId}/{pageNumber}")]
        public IActionResult GetPage(int readerId, int quranId, int pageNumber)
        {
            try
            {
                var response = _QuranService.GetQuraanPage(readerId, quranId, pageNumber);
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
