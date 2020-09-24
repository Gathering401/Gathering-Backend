using GatheringAPI.Models;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace GatheringAPI.Services
{
    public class DbUserRepo : IUser
    {
        private readonly UserManager<User> userManager;
        private readonly JWTToken tokenService;

        public DbUserRepo(UserManager<User> userManager, JWTToken tokenService)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
        }

        public async Task<UserDto> Authenticate(string userName, string password)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (await userManager.CheckPasswordAsync(user, password))
            {
                return new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Token = await tokenService.GetToken(user, TimeSpan.FromMinutes(30)),
                };
            }
            return null;
        }

        public async Task<UserDto> Register(RegisterData data, ModelStateDictionary modelState)
        {
            var user = new User
            {
                FirstName = data.FirstName,
                LastName = data.LastName,
                BirthDate = data.BirthDate,
                UserName = data.Username,
                PhoneNumber = data.PhoneNumber,
                Email = data.Email,
            };

            var result = await userManager.CreateAsync(user, data.Password);
            if (result.Succeeded)
            {
                if (data.Roles != null)
                {
                    await userManager.AddToRolesAsync(user, data.Roles);
                }
                return new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,

                };
            }
            foreach (var error in result.Errors)
            {
                var errorKey =
                    error.Code.Contains("Password") ? nameof(data.Password) :
                    error.Code.Contains("Email") ? nameof(data.Email) :
                    error.Code.Contains("UserName") ? nameof(data.Password) :
                    "";
                modelState.AddModelError(errorKey, error.Description);
            }
            return null;


        }
    }
    public interface IUser
    {
        Task<UserDto> Authenticate(string userName, string password);
        Task<UserDto> Register(RegisterData data, ModelStateDictionary modelState);
    };


}
