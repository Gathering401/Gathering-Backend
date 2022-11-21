using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GatheringAPI.Services
{
    public class DbUserRepo : IUser
    {
        private readonly UserManager<User> userManager;

        private readonly GatheringDbContext _context;

        private readonly JWTToken tokenService;

        public DbUserRepo(UserManager<User> userManager, JWTToken tokenService, GatheringDbContext context)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            _context = context;
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
                    Token = await tokenService.GetToken(user, TimeSpan.FromDays(30)),
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
                    FirstName = user.FirstName,
                    LastName = user.LastName,
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

        public async Task<bool> SaveStatus(EventInvite invite)
        {
            _context.Entry(invite).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveComment(EventComment comment)
        {
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public interface IUser
    {
        Task<UserDto> Authenticate(string userName, string password);

        Task<UserDto> Register(RegisterData data, ModelStateDictionary modelState);

        Task<bool> SaveStatus(EventInvite invite);
        Task<bool> SaveComment(EventComment comment);
    };


}
