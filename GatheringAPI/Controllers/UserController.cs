using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Services;
using GatheringAPI.Models.Api;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUser userService;

        public UserController(IUser userService)
        {
            this.userService = userService;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterData data)
        {
            Console.WriteLine("Yes we are getting here, but not doing much");
            UserDto user = await userService.Register(data, this.ModelState);
            if(!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            return user;
        }
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

    }
}
