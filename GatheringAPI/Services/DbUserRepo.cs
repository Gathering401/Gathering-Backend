using GatheringAPI.Data;
using GatheringAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Services
{
    public class DbUserRepo : IUser
    {
        private readonly GatheringDbContext _context;

        public DbUserRepo(GatheringDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            
        }

        public async Task<ActionResult<User>> FindAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task<ActionResult<IEnumerable<User>>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
    public interface IUser
    {
        Task<ActionResult<IEnumerable<User>>> GetAllAsync();
        Task<ActionResult<User>> FindAsync(long id);
        Task CreateAsync(User user);
    }

    
}
