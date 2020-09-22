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
    public class DbEventRepo: IEvent
    {
        private readonly GatheringDbContext _context;
       

        public DbEventRepo(GatheringDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<Event>>> GetAllAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<ActionResult<Event>> GetOneByIdAsync(long id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event == null)
            {
                return null;
            }

            return @event;
        }
    }

    public interface IEvent
    {
        Task<ActionResult<IEnumerable<Event>>> GetAllAsync();

        Task<ActionResult<Event>> GetOneByIdAsync(long id);
    }
}
