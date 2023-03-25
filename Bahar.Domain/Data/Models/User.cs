using System;

namespace BaharAlqeraat.Domain.Data.Models
{
    public partial class User
    {
        public User()
        {

        }
        public int Id { get; set; }
        public string DeviceToken { get; set; }
        public int SavedQuranPage { get; set; }
    }
}
