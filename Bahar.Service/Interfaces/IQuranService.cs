using BaharAlqeraat.Domain.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bahar.Service
{
    public interface IQuranService
    {
        Task<List<QuranReader>> ListReaders();
        Task<List<QuranLine>> SearchQuran(string searchValue);
        Task<QuranReader> GetReaderAsync(int id);
        byte[] GetQuraanPage(int readerId, int quranId, int pageNumber);
        string GetQuraanPageText(int readerId, int quranId, int pageNumber);
        byte[] GetQuraanPageFull(int readerId, int quranId, int pageNumber);
        void ConvertQuraanFull(int readerId, int quranId);
    }
}


