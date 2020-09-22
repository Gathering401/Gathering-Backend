using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GatheringAPI.Data;
using GatheringAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GatheringAPI.Services
{
    public class DbGroupRepo : IGroup
    {
        private readonly GatheringDbContext _context;

        public DbGroupRepo(GatheringDbContext context)
        {
            _context = context;
        }

        public Task<ActionResult<IEnumerable<Group>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }

    public interface IGroup
    {
        Task<ActionResult<IEnumerable<Group>>> GetAllAsync();
    }
}
