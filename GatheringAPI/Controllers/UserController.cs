using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GatheringAPI.Services;
using GatheringAPI.Models.Api;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using GatheringAPI.Models;
using System;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUser userService;
        public IConfiguration Configuration { get; }

        public UserController(IUser userService)
        {
            this.userService = userService;
        }
        private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // POST: api/User/Register
        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterData data)
        {
            UserDto user = await userService.Register(data, this.ModelState);
            if(!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            return user;
        }

        // POST: api/User/Login
        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginData data)
        {
            var user = await userService.Authenticate(data.Username, data.Password);
            if (user == null)
            {
                return Unauthorized();
            }
            return user;
        }

        // PUT: api/User/Preferences
        [HttpPut("Preferences")]
        public async Task<IActionResult> UpdatePreferences(User data)
        {
            if(UserId != data.Id)
                return BadRequest();

            bool DidUpdate = await userService.PutPreferences(UserId, data);

            if (!DidUpdate)
                return NotFound();

            return NoContent();
        }
    }
}
