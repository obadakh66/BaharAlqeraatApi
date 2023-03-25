using BaharAlqeraat.Domain;
using BaharAlqeraat.Domain.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Bahar.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task RefreshDeviceToken(string deviceToken)
        {
            var user =await _context.Users.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken);
            if (user == null)
            {
                User newUser = new User
                {
                    DeviceToken = deviceToken,
                    SavedQuranPage = 0
                };
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
            }
            else
            {
            user.DeviceToken = deviceToken;
            await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateSavedPageNumber(string deviceToken, int savedPageNumber)
        {
            var user =await _context.Users.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken);
            user.SavedQuranPage = savedPageNumber;
            await _context.SaveChangesAsync();
        }
        public async Task<User> GetUser(string deviceToken)
        {
            var user =await _context.Users.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken);
            return user;
        }

    }
}
