using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Models.Api;
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
        private readonly IGroupUser guRepo;

        public DbGroupRepo(GatheringDbContext context, IConfiguration configuration, IGroupUser groupUserRepo)
        {
            _context = context;
            Configuration = configuration;
            guRepo = groupUserRepo;
        }

        public async Task CreateAsync(Group group, long userId)
        {
            group.GroupUsers = new List<GroupUser>();
            group.GroupUsers.Add(new GroupUser { UserId = userId, Role = Role.owner });
            group.IsPublic = false;

            switch (group.GroupSize)
            {
                case GroupSizes.free:
                    group.MaxUsers = 20;
                    group.MaxEvents = 300;
                    group.IsPublic = true;
                    break;
                case GroupSizes.extraSmall:
                    group.MaxUsers = 50;
                    group.MaxEvents = 750;
                    break;
                case GroupSizes.small:
                    group.MaxUsers = 100;
                    group.MaxEvents = 1500;
                    break;
                case GroupSizes.medium:
                    group.MaxUsers = 250;
                    group.MaxEvents = 3750;
                    break;
                case GroupSizes.large:
                    group.MaxUsers = 1000;
                    group.MaxEvents = 15000;
                    break;
                case GroupSizes.infinite:
                    group.MaxUsers = -1;
                    group.MaxEvents = -1;
                    break;
                default:
                    Console.WriteLine("Should not get here.");
                    break;
            }

            _context.Groups.Add(@group);
            await _context.SaveChangesAsync();
        }

        public async Task<string> DeleteAsync(long id, long userId)
        {
            GroupUser user = await _context.GroupUsers.FindAsync(id, userId);

            if (user.Role == Role.owner)
            {
                IQueryable<Group> userGroups = UserGroups(userId);
                var @group = userGroups.FirstOrDefault(g => g.GroupId == id);

                if (@group == null)
                {
                    return "null";
                }
                _context.Entry(@group).State = EntityState.Deleted;

                await _context.SaveChangesAsync();

                return "true";
            }
            else
                return "false";
            
        }

        public GroupDto Find(long id, long userId, GroupUser currentUser)
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
                            Cost = ge.Event.Cost,
                            Location = ge.Event.Location
                        })
                        .ToList(),
                    GroupUsers = (currentUser.Role != Role.admin && currentUser.Role != Role.owner) ? null : group.GroupUsers
                        .Select(gu => new GroupUserDto
                        {
                            UserId = gu.UserId,
                            User = null,
                            GroupId = gu.GroupId,
                            Group = null,
                            Role = gu.Role,
                            RoleString = gu.Role.ToString()
                        })
                        .ToList(),
                    RequestsToJoin = (currentUser.Role != Role.admin && currentUser.Role != Role.owner) ? null : group.RequestsToJoin
                        .Select(jr => new JoinRequestDto
                        {
                            UserName = jr.User.UserName,
                            FirstName = jr.User.FirstName,
                            LastName = jr.User.LastName,
                            Status = jr.Status,
                            UserId = jr.UserId
                        })
                        .ToList(),
                    MaxUsers = group.MaxUsers,
                    MaxEvents = group.MaxEvents
                })
                .FirstOrDefault();
        }

        public async Task<Group> GetGroup(long id)
        {
            return await _context.Groups.FindAsync(id);
        }

        public async Task<IEnumerable<GroupDto>> SearchGroupsByString(string searchFor)
        {
            return await _context.Groups
                .Select(@group => new GroupDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Location = group.Location
                })
                .Where(g => g.GroupName.StartsWith(searchFor))
                .ToListAsync();
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
                    Location = group.Location
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

        public async Task<bool> DeleteEventAsync(long groupId, long eventId, long UserId)
        {
            var @event = await _context.Events
                .Include(e => e.EventHost)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (HostMatchesCurrent(UserId, @event) == false)
            {
                GroupUser currentUser = await guRepo.GetGroupUser(groupId, UserId);
                if (currentUser.Role == Role.user || currentUser.Role == Role.creator)
                    return false;
            }

            var groupEvent = await _context.GroupEvents.FindAsync(groupId, eventId);

            _context.GroupEvents.Remove(groupEvent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEventAsync(long groupId, Event @event, long UserId)
        {
            @event = await _context.Events
                .Include(e => e.EventHost)
                .FirstOrDefaultAsync(e => e.EventId == @event.EventId);

            if (HostMatchesCurrent(UserId, @event) == false)
            {
                GroupUser currentUser = await guRepo.GetGroupUser(groupId, UserId);
                if(currentUser.Role == Role.user || currentUser.Role == Role.creator)
                    return false;
            }
                

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

        public async Task AddUserAsync(long groupId, string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            var groupUser = new GroupUser
            {
                GroupId = groupId,
                UserId = user.Id,
                Role = Role.user
            };

            _context.GroupUsers.Add(groupUser);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveUserAsync(GroupUser current, GroupUser adjusted)
        {
            if(current.Role == Role.owner)
            {
                _context.GroupUsers.Remove(adjusted);
                await _context.SaveChangesAsync();

                return true;
            }
            else if(current.Role == Role.admin && adjusted.Role != Role.owner && adjusted.Role != Role.admin)
            {
                _context.GroupUsers.Remove(adjusted);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public string _accountSid = null;
        public string _authToken = null;
        public string _phone = null;

        public void SendInvites(Event @event)
        {
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

                    try
                    {
                        MessageResource.Create(
                         body: $"You've been invited to {@event.EventName}! Please reply with your RSVP - 1 for Yes, 2 for No, 3 for Maybe, 4 to get more Details, 5 for event description, or 6 to ask a question. Your response will apply to your most recent invitation without a response.",
                          from: new Twilio.Types.PhoneNumber($"+1{_phone}"),
                          to: new Twilio.Types.PhoneNumber($"+1{user.User.PhoneNumber}")
                      );
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    var invitation = new EventInvite
                    {
                        Event = @event,
                        EventId = @event.EventId,
                        UserId = user.UserId,
                        Status = RSVPStatus.Pending,
                        User = user.User
                    };

                    user.User.Invites.Add(invitation);
                    _context.Entry(user.User).State = EntityState.Modified;

                }
            }
            _context.SaveChanges();
        }

        public async Task CreateEventAsync(EventRepeat @event, long userId, long groupId)
        {
            DateTime repeatDate = @event.Event.Start;
            switch(@event.ERepeat)
            {
                case Repeat.Weekly:
                    DateTime endDate = repeatDate.AddYears(1);
                    while(repeatDate < endDate)
                    {
                        var IndividualEvent = new Event()
                        {
                            EventName = @event.Event.EventName,
                            Start = repeatDate,
                            Food = @event.Event.Food,
                            Cost = @event.Event.Cost,
                            Location = @event.Event.Location,
                            Description = @event.Event.Description
                        };
                        await CreateIndividualEventAsync(IndividualEvent, userId, groupId);
                        repeatDate = repeatDate.AddDays(7);
                    }
                    break;

                default:
                    Console.WriteLine("Something went wrong.");
                    break;
            }
        }

        public async Task CreateIndividualEventAsync(Event @event, long userId, long groupId)
        {
            @event.EventHost = new HostedEvent
            {
                UserId = userId,
                EventId = @event.EventId
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            await AddEventAsync(groupId, @event.EventId);
            // SendInvites(@event);
        }

        public async Task<long> FindUserIdByUserName(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            return user.Id;
        }

        public bool HostMatchesCurrent(long current, Event @event)
        {
            return current == @event.EventHost.UserId;
        }

        public async Task AddUserAsync(long groupId, string username, Role newRole)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            
            var groupUser = new GroupUser
            {
                GroupId = groupId,
                UserId = user.Id,
                Role = newRole
            };

            _context.GroupUsers.Add(groupUser);
            await _context.SaveChangesAsync();
        }

        public async Task RequestToJoinGroupById(long groupId, long userId)
        {
            Group currentGroup = await GetGroup(groupId);

            if(currentGroup.IsPublic == false)
            {
                JoinRequest joinRequest = new JoinRequest
                {
                    GroupId = groupId,
                    UserId = userId
                };

                _context.JoinRequests.Add(joinRequest);
                await _context.SaveChangesAsync();
            }
            else
            {
                var groupUser = new GroupUser
                {
                    GroupId = groupId,
                    UserId = userId,
                    Role = Role.user
                };

                _context.GroupUsers.Add(groupUser);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RespondToGroupJoinRequest(long groupId, long userId, JoinStatus status)
        {
            if(status == JoinStatus.accepted)
            {
                GroupUser groupUser = new GroupUser
                {
                    GroupId = groupId,
                    UserId = userId,
                    Role = Role.user
                };

                _context.GroupUsers.Add(groupUser);
                await _context.SaveChangesAsync();
            }

            var joinRequest = await _context.JoinRequests.FindAsync(groupId, userId);
            _context.JoinRequests.Remove(joinRequest);
            await _context.SaveChangesAsync();
        }
    }

    public interface IGroup
    {
        IEnumerable<GroupDto> GetAll(long userId);

        GroupDto Find(long id, long userId, GroupUser currentUser);
        Task<Group> GetGroup(long id);
        Task<bool> UpdateEventAsync(long groupId, Event @event, long UserId);
        Task CreateAsync(Group group, long userId);

        Task<string> DeleteAsync(long id, long userId);

        Task<bool> UpdateAsync(Group group, long userId);

        Task AddEventAsync(long groupId, long eventId);

        Task<bool> DeleteEventAsync(long groupId, long eventId, long UserId);
        Task AddUserAsync(long groupId, string userName);
        Task AddUserAsync(long groupId, string username, Role newRole);
        Task<bool> RemoveUserAsync(GroupUser current, GroupUser adjusted);

        void SendInvites(Event @event);
        Task CreateEventAsync(EventRepeat @event, long userId, long groupId);
        Task CreateIndividualEventAsync(Event @event, long userId, long groupId);
        Task<long> FindUserIdByUserName(string userName);

        bool HostMatchesCurrent(long current, Event @event);
        Task<IEnumerable<GroupDto>> SearchGroupsByString(string searchFor);
        Task RequestToJoinGroupById(long groupId, long userId);
        Task RespondToGroupJoinRequest(long groupId, long userId, JoinStatus status);
    }

}
