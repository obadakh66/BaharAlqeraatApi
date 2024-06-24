using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public interface IBackgroundServiceStatus
{
    bool IsRunning { get; set; }
    int? StartingSurah { get; set; }
    int? StartingAyah { get; set; }
    List<int> QuransIds { get; set; }
}

public class BackgroundServiceStatus : IBackgroundServiceStatus
{
    public bool IsRunning { get; set; }
    public int? StartingSurah { get; set; } = null;
    public int? StartingAyah { get; set; } = null;
    public List<int> QuransIds { get; set; } = new List<int>();
}
public class AudioScrapingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackgroundServiceStatus _backgroundServiceStatus;

    public AudioScrapingBackgroundService(
        IServiceProvider serviceProvider,
        IBackgroundServiceStatus backgroundServiceStatus)
    {
        _serviceProvider = serviceProvider;
        _backgroundServiceStatus = backgroundServiceStatus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retryDelay = TimeSpan.FromSeconds(7); // Initial retry delay
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_backgroundServiceStatus.IsRunning)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var audioScrapingService = scope.ServiceProvider.GetRequiredService<IAudioScrapingService>();
                        await audioScrapingService.ScrapeAndDownloadAudioFilesAsync(_backgroundServiceStatus.QuransIds,_backgroundServiceStatus.StartingSurah,_backgroundServiceStatus.StartingAyah);
                    }
                    // Reset retry delay after successful operation
                    retryDelay = TimeSpan.FromSeconds(7);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AudioScrapingBackgroundService: {ex.Message}");
                // Implement backoff logic
                retryDelay = TimeSpan.FromSeconds(Math.Min(retryDelay.TotalSeconds * 2, 600)); // Example: Exponential backoff with a maximum delay
            }
            // Wait before retrying the operation, respecting the stopping token
            await Task.Delay(retryDelay, stoppingToken);
        }
    }

}
