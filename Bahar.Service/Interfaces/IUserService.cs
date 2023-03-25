using BaharAlqeraat.Domain.Data.Models;
using System.Threading.Tasks;

namespace Bahar.Service
{
    public interface IUserService
    {
        Task RefreshDeviceToken(string deviceToken);
        Task UpdateSavedPageNumber(string deviceToken, int savedPageNumber);
        Task<User> GetUser(string deviceToken);
    }
}


