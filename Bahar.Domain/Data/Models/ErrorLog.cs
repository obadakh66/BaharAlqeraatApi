using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaharAlqeraat.Domain.Data.Models
{
    public partial class ErrorLog
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public int QuranId { get; set; }
        public int SurahId { get; set; }
        public int AyahId { get; set; }
        public string Error { get; set; }
    }
}
