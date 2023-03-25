using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaharAlqeraat.Domain.Data.Models
{
    public partial class QuranReader
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Quran> Qurans { get; set; } 
    }
}
