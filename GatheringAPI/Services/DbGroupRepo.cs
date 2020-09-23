using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Models.Api;
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

        public GroupDto Find(long id)
        {
            return _context.Groups
                .Where(g => g.GroupId == id)
                .Select(@group => new GroupDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Location = group.Location,
                    GroupEvents = group.GroupEvents
                        .Select(ge => new GroupEventDto
                        {
                            EventName = ge.Event.EventName,
                            Start = ge.Event.Start,
                            End = ge.Event.End,
                            DayOfMonth = ge.Event.DayOfMonth,
                            Cost = ge.Event.Cost,
                            Location = ge.Event.Location
                        })
                        .ToList()
                })
                .FirstOrDefault();
        }

        public IEnumerable<GroupDto> GetAll()
        {
            return _context.Groups
                .Select(@group => new GroupDto
                {
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Location = group.Location,
                    GroupEvents = group.GroupEvents
                    .Select(e => new GroupEventDto
                    {
                        EventName = e.Event.EventName,
                        Start = e.Event.Start,
                        End = e.Event.End,
                        DayOfMonth = e.Event.DayOfMonth,
                        Cost = e.Event.Cost,
                        Location = e.Event.Location,

                    }).ToList(),
                });
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

        public async Task<bool> UpdateEventAsync(long groupId, Event @event)
        {
            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!await EventExists(groupId, @event))
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

        private async Task<bool> EventExists(long groupId, Event @event)
        {
            var @group = await _context.Groups.FindAsync(groupId);
            if (@group.GroupEvents.Where(ge => ge.EventId == @event.EventId).Count() == 0)
            {
                return false;
            }
            return true;
        }
    }

    public interface IGroup
    {
        IEnumerable<GroupDto> GetAll();

        GroupDto Find(long id);
        Task<bool> UpdateEventAsync(long groupId, Event @event);
        Task CreateAsync(Group group);

        Task<Group> DeleteAsync(long id);

        Task<bool> UpdateAsync(Group group);

        Task AddEventAsync(long groupId, long eventId);

        Task DeleteEventAsync(long groupId, long eventId);
    }
}
