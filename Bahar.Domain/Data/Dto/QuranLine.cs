using BaharAlqeraat.Domain.Data.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaharAlqeraat.Domain.Data.Dtos
{
    public partial class QuranLineResponse
    {
        public List<QuranLine> Result { get; set; }
        public int TotalCount { get; set; }
    }
    public class AudioDownloadRequest
    {
        public byte[] AudioBytes { get; set; }
        public int QuranId { get; set; }
        public int Surah { get; set; }
        public int Ayah { get; set; }
    }
}
