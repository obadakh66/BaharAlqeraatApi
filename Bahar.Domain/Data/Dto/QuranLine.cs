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
}
