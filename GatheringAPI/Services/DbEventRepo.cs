using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Services
{
    public class DbEventRepo : IEvent
    {
        private readonly GatheringDbContext _context;


        public DbEventRepo(GatheringDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<IEnumerable<EventDto>>> GetAllAsync()
        {
            return await _context.Events
                .Select(e => new EventDto
                {
                    EventName = e.EventName,
                    Start = e.Start,
                    End = e.End,
                    Cost = e.Cost,
                    Location = e.Location,
                    DayOfMonth = e.DayOfMonth,
                    Attending = e.Attending
                        .Select(a => new AttendingDto
                        {
                            Name = $"{a.User.FirstName} {a.User.LastName}",
                            Status = a.Status.ToString()
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<ActionResult<Event>> GetOneByIdAsync(long id)
        {
            var @event = await _context.Events
                .Include(e => e.Attending)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return null;
            }

            return @event;
        }

        public async Task<bool> UpdateByIdAsync(Event @event)
        {
            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {
                if (!await EventExists(@event.EventId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        private async Task<bool> EventExists(long id)
        {
            return await _context.Events.AnyAsync(e => e.EventId == id);
        }
    }

    public interface IEvent
    {
        Task<ActionResult<IEnumerable<EventDto>>> GetAllAsync();

        Task<ActionResult<Event>> GetOneByIdAsync(long id);

        Task<bool> UpdateByIdAsync(Event @event);

    }
}
