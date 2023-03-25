using Bahar.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaharAlqeraat.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {

        private readonly ILogger<UserController> _logger;
        private IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("{deviceToken}")]
        public async Task<IActionResult> UpdateToken(string deviceToken)
        {
            try
            {
                await _userService.RefreshDeviceToken(deviceToken);
                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost("{deviceToken}/{number}")]
        public async Task<IActionResult> UpdateNumber(string deviceToken,int number)
        {
            try
            {
                await _userService.UpdateSavedPageNumber(deviceToken,number);
                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet("{deviceToken}")]
        public async Task<IActionResult> GetUser(string deviceToken)
        {
            try
            {
                var user= await _userService.GetUser(deviceToken);
                return Ok(user);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
