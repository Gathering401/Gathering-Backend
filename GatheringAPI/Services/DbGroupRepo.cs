using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace GatheringAPI.Services
{
    public class DbGroupRepo : IGroup
    {
        public IConfiguration Configuration { get; }

        private readonly GatheringDbContext _context;

        public DbGroupRepo(GatheringDbContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public async Task CreateAsync(Group group, long userId)
        {
            group.GroupUsers = new List<GroupUser>();
            group.GroupUsers.Add(new GroupUser { UserId = userId });
            _context.Groups.Add(@group);
            await _context.SaveChangesAsync();
        }

        public async Task<Group> DeleteAsync(long id, long userId)
        {
            IQueryable<Group> userGroups = UserGroups(userId);
            var @group = userGroups.FirstOrDefault(g => g.GroupId == id);

            if (@group == null)
            {
                return null;
            }
            _context.Entry(@group).State = EntityState.Deleted;

            await _context.SaveChangesAsync();

            return null;
        }

        public GroupDto Find(long id, long userId)
        {
            IQueryable<Group> userGroups = UserGroups(userId);
            return userGroups
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
                            EventId = ge.Event.EventId,
                            EventName = ge.Event.EventName,
                            Start = ge.Event.Start,
                            End = ge.Event.End,
                            DayOfMonth = ge.Event.DayOfMonth,
                            Cost = ge.Event.Cost,
                            Location = ge.Event.Location
                        })
                        .ToList(),
                    GroupUsers = group.GroupUsers
                        .Select(gu => new UserDto
                        {
                            Username = gu.User.UserName,
                            Id = gu.User.Id
                        })
                        .ToList()
                })
                .FirstOrDefault();
        }

        public IEnumerable<GroupDto> GetAll(long userId)
        {
            IQueryable<Group> userGroups = UserGroups(userId);
            return userGroups
                .Select(@group => new GroupDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Location = group.Location,
                    GroupEvents = group.GroupEvents
                        .Select(e => new GroupEventDto
                        {
                            EventId = e.Event.EventId,
                            EventName = e.Event.EventName,
                            Start = e.Event.Start,
                            End = e.Event.End,
                            DayOfMonth = e.Event.DayOfMonth,
                            Cost = e.Event.Cost,
                            Location = e.Event.Location,

                        })
                        .ToList(),
                    GroupUsers = group.GroupUsers
                        .Select(gu => new UserDto
                        {
                            Username = gu.User.UserName,
                            Id = gu.User.Id
                        })
                        .ToList()
                });
        }

        private IQueryable<Group> UserGroups(long userId)
        {
            return _context.Groups.Where(g => g.GroupUsers.Any(u => u.UserId == userId));
        }

        public async Task<bool> UpdateAsync(Group @group, long userId)
        {
            IQueryable<Group> userGroups = UserGroups(userId);
            if (!userGroups.Any(g => g.GroupId == @group.GroupId))
            {
                return false;
            }
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

        public async Task AddUserAsync(long groupId, long userId)
        {
            var groupUser = new GroupUser
            {
                GroupId = groupId,
                UserId = userId
            };

            _context.GroupUsers.Add(groupUser);
            await _context.SaveChangesAsync();
        }

        public string _accountSid = null;
        public string _authToken = null;
        public string _phone = null;

        public void SendInvites(long eventId)
        {
            var @event = _context.Events.Find(eventId);

            _phone = Configuration["Twilio:phone"];
            _accountSid = Configuration["Twilio:accountSid"];
            _authToken = Configuration["Twilio:authToken"];

            if (String.IsNullOrEmpty(_phone))
                throw new NullTwilioPhoneException();

            if (String.IsNullOrEmpty(_accountSid))
                throw new NullTwilioSidException();

            if (String.IsNullOrEmpty(_authToken))
                throw new NullTwilioTokenException();

            TwilioClient.Init(_accountSid, _authToken);

            foreach (var group in @event.InvitedGroups)
            {
                var currGroup = _context.Groups
                    .Include(g => g.GroupUsers)
                    .ThenInclude(gu => gu.User)
                    .ThenInclude(u => u.Invites)
                    .FirstOrDefault(g => g.GroupId == group.GroupId);

                foreach (var user in currGroup.GroupUsers)
                {
                    if (String.IsNullOrEmpty(user.User.PhoneNumber))
                        continue;

                    var message = MessageResource.Create(
                        body: $"You've been invited to {@event.EventName}! Please reply with your RSVP - 1 for Yes, 2 for No, 3 for Maybe, 4 to get more Details, 5 for event description, or 6 to ask a question. Your response will apply to your most recent invitation without a response.",
                        from: new Twilio.Types.PhoneNumber($"+1{_phone}"),
                        to: new Twilio.Types.PhoneNumber($"+1{user.User.PhoneNumber}")
                        );

                    var invitation = new EventInvite
                    {
                        Event = @event,
                        EventId = eventId,
                        UserId = user.UserId,
                        Status = RSVPStatus.Pending,
                        User = user.User
                    };

                    user.User.Invites.Add(invitation);
                    _context.Entry(user.User).State = EntityState.Modified;

                    Console.WriteLine(message.Sid);
                }
            }
            _context.SaveChanges();
        }

        public async Task CreateEventAsync(Event @event, long userId, long groupId)
        {
            @event.EventHost = new HostedEvent
            {
                UserId = userId,
                EventId = @event.EventId
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            await AddEventAsync(groupId, @event.EventId);
        }
    }

    public interface IGroup
    {
        IEnumerable<GroupDto> GetAll(long userId);

        GroupDto Find(long id, long userId);
        Task<bool> UpdateEventAsync(long groupId, Event @event);
        Task CreateAsync(Group group, long userId);

        Task<Group> DeleteAsync(long id, long userId);

        Task<bool> UpdateAsync(Group group, long userId);

        Task AddEventAsync(long groupId, long eventId);

        Task DeleteEventAsync(long groupId, long eventId);
        Task AddUserAsync(long groupId, long userId);

        void SendInvites(long eventId);
        Task CreateEventAsync(Event @event, long userId, long groupId);
    }

}
