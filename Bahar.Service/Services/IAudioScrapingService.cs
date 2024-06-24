using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAudioScrapingService
{
    Task ScrapeAndDownloadAudioFilesAsync(List<int> quransIds,int? startingSurah = null, int? startingAyah = null);
    Task LogSuccessfulFiles(int ayahId, int quranId, int surahId, bool isSaved);
    Task LogError(int ayahId, int quranId, int surahId, string error);
}