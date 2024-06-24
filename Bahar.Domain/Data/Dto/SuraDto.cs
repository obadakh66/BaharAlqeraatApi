using BaharAlqeraat.Domain.Data.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaharAlqeraat.Domain.Data.Dtos
{
    public partial class SuraDto
    {
        public int PageNumber { get; set; }
        public int SuraNumber { get; set; }
        public string SuraName { get; set; }
        public List<AyahDto> Ayat { get; set; }
    }
    public partial class SurasDto
    {
        public int PageNumber { get; set; }
        public List<SuraDto> Suras { get; set; }
    }
   
    
    public partial class AyahDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int PageNumber { get; set; }
        public int SuraNumber { get; set; }
        public string SuraName { get; set; }
        public string Content { get; set; }
        public string ContentSimple { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
