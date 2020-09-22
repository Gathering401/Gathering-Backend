using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatheringAPI.Data;
using GatheringAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatheringAPI.Services
{
    public class DbGroupRepo : IGroup
    {
        private readonly GatheringDbContext _context;

        public DbGroupRepo(GatheringDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Group>>> GetAllAsync()
        {
            return await _context.Groups.ToListAsync();
        }
    }

    public interface IGroup
    {
        Task<ActionResult<IEnumerable<Group>>> GetAllAsync();
    }
}
