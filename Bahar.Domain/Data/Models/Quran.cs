using System.ComponentModel.DataAnnotations;

namespace BaharAlqeraat.Domain.Data.Models
{
    public partial class Quran
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int QuranReaderId { get; set; }
        public QuranReader QuranReader { get; set; }
    }
}
