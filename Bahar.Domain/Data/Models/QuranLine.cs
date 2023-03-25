using System.ComponentModel.DataAnnotations;

namespace BaharAlqeraat.Domain.Data.Models
{
    public partial class QuranLine
    {
        [Key]
        public int Id { get; set; }
        public int Number { get; set; }
        public int PageNumber { get; set; }
        public int SuraNumber { get; set; }
        public string SuraName { get; set; }
        public string Content { get; set; }
        public string ContentSimple { get; set; }
    }
}
