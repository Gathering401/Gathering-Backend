using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Services
{
    public class DbUserRepo : IUser
    {
        private readonly UserManager<User> userManager;

        public async Task<UserDto> Authenticate(string userName, string password)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (await userManager.CheckPasswordAsync(user, password))
            {
                return new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,


                };
            }
            return null;
        }

        public async Task<UserDto> Register(RegisterData data, ModelStateDictionary modelState)
        {
            var user = new User
            {
                UserName = data.Username,
                Email = data.Email
            };

            var result = await userManager.CreateAsync(user, data.Password);
            if(result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, data.Roles);
                return new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,

                };
            }
            foreach(var error in result.Errors)
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
