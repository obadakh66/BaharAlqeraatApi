using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAudioScrapingService
{
    Task ScrapeAndDownloadAudioFilesAsync(List<int> quransIds, CancellationToken cancellationToken, int? startingSurah = null, int? startingAyah = null);
    Task LogSuccessfulFiles(int ayahId, int quranId, int surahId, bool isSaved, CancellationToken cancellationToken);
    Task LogError(int ayahId, int quranId, int surahId, string error, CancellationToken cancellationToken);
}