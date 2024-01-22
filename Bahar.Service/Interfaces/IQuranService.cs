using BaharAlqeraat.Domain.Data.Dtos;
using BaharAlqeraat.Domain.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bahar.Service
{
    public interface IQuranService
    {
        Task<List<QuranReader>> ListReaders();
        Task<QuranLineResponse> SearchQuran(string searchValue, int pageNumber, int pageSize);
        Task<QuranReader> GetReaderAsync(int id);
        Task<byte[]> GetQuraanPageAsync(int readerId, int quranId, int pageNumber);
        string GetQuraanPageText(int readerId, int quranId, int pageNumber);
        byte[] GetQuraanPageFull(int readerId, int quranId, int pageNumber);
        void ConvertQuraanFull(int readerId, int quranId);
        byte[] GetQuraanPageCropped(int readerId, int quranId, int pageNumber);
        Task<SuraDto> GetSuraAsync(int pageId);
        Task ScrapeAndDownloadAudioFilesAsync();
    }
}


