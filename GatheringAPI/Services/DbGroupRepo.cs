using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task CreateAsync(Group group)
        {
            _context.Groups.Add(@group);
            await _context.SaveChangesAsync();
        }

        public async Task<Group> DeleteAsync(long id)
        {
            var @group = await _context.Groups.FindAsync(id);

            if (@group == null)
            {
                return null;
            }

            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();

            return @group;
        }

        public async Task<ActionResult<Group>> FindAsync(long id)
        {
            var @group = await _context.Groups.FindAsync(id);
            return @group;
        }

        public async Task<ActionResult<IEnumerable<Group>>> GetAllAsync()
        {
            return await _context.Groups
                .Include(g => g.GroupEvents)
                .ThenInclude(ge => ge.Event)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(Group @group)
        {
            _context.Entry(@group).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(@group.GroupId))
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

        private bool GroupExists(long id)
        {
            return _context.Groups.Any(g => g.GroupId == id);
        }

        public async Task AddEventAsync(long groupId, long eventId)
        {
            var groupEvent = new GroupEvent
            {
                EventId = eventId,
                GroupId = groupId
            };

            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(long groupId, long eventId)
        {
            var groupEvent = await _context.GroupEvents.FindAsync(groupId, eventId);

            _context.GroupEvents.Remove(groupEvent);
            await _context.SaveChangesAsync();
        }
    }

    public interface IGroup
    {
        Task<ActionResult<IEnumerable<Group>>> GetAllAsync();

        Task<ActionResult<Group>> FindAsync(long id);

        Task CreateAsync(Group group);

        Task<Group> DeleteAsync(long id);

        Task<bool> UpdateAsync(Group group);

        Task AddEventAsync(long groupId, long eventId);

        Task DeleteEventAsync(long groupId, long eventId);
    }
}
