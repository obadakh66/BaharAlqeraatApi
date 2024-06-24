using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaharAlqeraat.Controllers
{
    [ApiController]
    [Route("api/background-service")]
    public class BackgroundServiceController : ControllerBase
    {
        private readonly IBackgroundServiceStatus _backgroundServiceStatus;
        private readonly IAudioScrapingService _service;

        public BackgroundServiceController(IBackgroundServiceStatus backgroundServiceStatus, IAudioScrapingService service)
        {
            _backgroundServiceStatus = backgroundServiceStatus;
            _service = service;
        }

        [HttpPost("start")]
        public IActionResult StartBackgroundService(List<int> quransIds, int? startingSurah = null, int? startingAyah = null)
        {
            _backgroundServiceStatus.IsRunning = true;
            _backgroundServiceStatus.StartingSurah= startingSurah;
            _backgroundServiceStatus.StartingAyah= startingAyah;
            _backgroundServiceStatus.QuransIds= quransIds;
            return Ok("Background service started.");
        }
        [HttpPost("start-service")]
        public async Task StartServiceAsync(List<int> quransIds, int? startingSurah = null, int? startingAyah = null)
        {
             await _service.ScrapeAndDownloadAudioFilesAsync(quransIds,startingSurah,startingAyah);
        }

        [HttpPost("stop")]
        public IActionResult StopBackgroundService()
        {
            _backgroundServiceStatus.IsRunning = false;
            _backgroundServiceStatus.StartingSurah = null;
            _backgroundServiceStatus.StartingAyah = null;
            _backgroundServiceStatus.QuransIds = new List<int>();
            return Ok("Background service stopped.");
        }
    }

}
