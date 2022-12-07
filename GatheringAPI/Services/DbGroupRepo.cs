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
            group.GroupRepeatedEvents = new List<GroupRepeatedEvent>();

            switch (group.GroupSize)
            {
                case GroupSizes.free:
                    group.MaxUsers = 50;
                    group.MaxEvents = 100;
                    break;
                case GroupSizes.large:
                    group.MaxUsers = 500;
                    group.MaxEvents = 2500;
                    break;
                case GroupSizes.infinite:
                    group.MaxUsers = -1;
                    group.MaxEvents = -1;
                    break;
                default:
                    // TODO ===== need to throw this eventually
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
            UserDto groupOwner = _context.GroupUsers
                .Where(gu => gu.Role == Role.owner && gu.GroupId == id)
                .Select(gu => new UserDto
                {
                    Username = gu.User.UserName,
                    FirstName = gu.User.FirstName,
                    LastName = gu.User.LastName,
                    Id = gu.User.Id
                })
                .FirstOrDefault();

            IQueryable<Group> userGroups = UserGroups(userId);
            return userGroups
                .Where(g => g.GroupId == id)
                .Select(@group => new GroupDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    Description = group.Description,
                    Location = group.Location,
                    GroupEvents = null,
                    GroupRepeatedEvents = group.GroupRepeatedEvents
                        .Select(gre => new RepeatedEventDto
                        {
                            EventName = gre.EventRepeat.EventName,
                            EventId = gre.EventRepeat.EventRepeatId,
                            ERepeat = gre.EventRepeat.ERepeat,
                            RepeatString = gre.EventRepeat.ERepeat.ToString(),
                            DayOfWeek = gre.EventRepeat.DayOfWeek,
                            DayOfMonth = gre.EventRepeat.DayOfMonth,
                            MonthOfYear = gre.EventRepeat.MonthOfYear,
                            FirstEventDate = gre.EventRepeat.FirstEventDate,
                            EndEventDate = gre.EventRepeat.EndEventDate
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
                    MaxEvents = group.MaxEvents,
                    Owner = groupOwner
                })
                .FirstOrDefault();
        }

        private IEnumerable<GroupEventDto> FindAllGroupEvents(long id, long userId)
        {
            IQueryable<Group> userGroups = UserGroups(userId);
            return userGroups
                .Where(g => g.GroupId == id)
                .Select(g => g.GroupEvents
                    .Select(ge => new GroupEventDto()
                    {
                        EventId = ge.EventId,
                        ERepeat = ge.Event.ERepeat,
                        EventName = ge.Event.EventName,
                        Start = ge.Event.Start,
                        Location = ge.Event.Location,
                        Cost = ge.Event.Cost,
                        End = ge.Event.End
                    })).FirstOrDefault();
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
                    Location = group.Location,
                    Owner = _context.GroupUsers.Where(gu => gu.Role == Role.owner && gu.GroupId == group.GroupId)
                        .Select(gu => new UserDto
                        {
                            Username = gu.User.UserName,
                            FirstName = gu.User.FirstName,
                            LastName = gu.User.LastName,
                            Id = gu.User.Id
                        })
                        .FirstOrDefault()
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

        public async Task AddEventAsync(long groupId, Event @event)
        {
            var groupEvent = new GroupEvent
            {
                EventId = @event.EventId,
                GroupId = groupId
            };

            _context.GroupEvents.Add(groupEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteRepeatedEventAsync(long groupId, long eventId)
        {
            long eventRepeatId = await _context.RepeatedEvents
                .Where(re => re.EventId == eventId)
                .Select(re => re.EventRepeatId)
                .FirstOrDefaultAsync();

            List<Event> events = await _context.RepeatedEvents
                .Where(re => re.EventRepeatId == eventRepeatId)
                .Select(re => re.Event)
                .ToListAsync();

            for (var i = 0; i < events.Count; i++)
            {
                await DeleteIndividualEventAsync(groupId, events[i].EventId);
            }

            var groupRepeatedEvent = await _context.GroupRepeatedEvents
                .FindAsync(groupId, eventRepeatId);

            if (groupRepeatedEvent == null)
                return false;

            _context.GroupRepeatedEvents.Remove(groupRepeatedEvent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteIndividualEventAsync(long groupId, long eventId)
        {
            var groupEvent = await _context.GroupEvents.FindAsync(groupId, eventId);
            if (groupEvent == null)
                return false;

            _context.GroupEvents.Remove(groupEvent);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateRepeatedEventAsync(long groupId, RepeatedEvent @event)
        {
            List<Event> events = await _context.RepeatedEvents
                .Where(re => re.EventRepeatId == @event.EventRepeatId)
                .Select(re => re.Event)
                .ToListAsync();

            foreach (var IndividualEvent in events)
            {
                IndividualEvent.EventName = @event.Event.EventName;
                IndividualEvent.Food = @event.Event.Food;
                IndividualEvent.Cost = @event.Event.Cost;
                IndividualEvent.Description = @event.Event.Description;
                IndividualEvent.Location = @event.Event.Location;

                bool result = await UpdateIndividualEventAsync(groupId, IndividualEvent);
                if (!result) return false;
            }
            bool repeatedResult = await UpdateDBRepeatedEventAsync(groupId, @event.EventRepeat);
            if (!repeatedResult) return false;
            return true;
        }

        public async Task<bool> UpdateIndividualEventAsync(long groupId, Event @event)
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

        public async Task<bool> UpdateDBRepeatedEventAsync(long groupId, EventRepeat @event)
        {
            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!await RepeatedEventExists(groupId, @event))
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

        private async Task<bool> RepeatedEventExists(long groupId, EventRepeat @event)
        {
            var @group = await _context.Groups.FindAsync(groupId);
            if (@group.GroupRepeatedEvents.Where(gre => gre.EventRepeatId == @event.EventRepeatId).Count() == 0)
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
            if (current.Role == Role.owner)
            {
                _context.GroupUsers.Remove(adjusted);
                await _context.SaveChangesAsync();

                return true;
            }
            else if (current.Role == Role.admin && adjusted.Role != Role.owner && adjusted.Role != Role.admin)
            {
                _context.GroupUsers.Remove(adjusted);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
        public async Task CreateInvites(EventRepeat eventRepeat, long groupId, long userId)
        {
            List<GroupUser> groupUsers = await _context.GroupUsers.Where(gu => gu.GroupId == groupId).ToListAsync();

            for(int i = 0; i < groupUsers.Count; i++)
            {
                EventInvite eventInvite = new EventInvite
                {
                    EventRepeatId = eventRepeat.EventRepeatId,
                    UserId = groupUsers[i].UserId,
                    Status = groupUsers[i].UserId == userId ? RSVPStatus.Accepted : RSVPStatus.Pending
                };

                _context.EventInvites.Add(eventInvite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CreateEventAsync(RepeatedEvent @event, long userId, long groupId)
        {
            DateTime repeatDate = @event.Event.Start;
            DateTime endDate;

            switch (@event.EventRepeat.ERepeat)
            {
                case Repeat.Weekly:
                    endDate = repeatDate.AddYears(3);

                    var weeklyEventRepeat = new EventRepeat()
                    {
                        EventName = @event.Event.EventName,
                        ERepeat = @event.EventRepeat.ERepeat,
                        Location = @event.Event.Location,
                        Description = @event.Event.Description,
                        DayOfWeek = @event.Event.Start.DayOfWeek,
                        FirstEventDate = @event.Event.Start,
                        EndEventDate = endDate
                    };
                    _context.EventRepeats.Add(weeklyEventRepeat);
                    await _context.SaveChangesAsync();
                    await CreateInvites(weeklyEventRepeat, groupId, userId);

                    while (repeatDate < endDate)
                    {
                        var weeklyEvent = new Event()
                        {
                            EventName = @event.Event.EventName,
                            Start = repeatDate,
                            Food = @event.Event.Food,
                            Cost = @event.Event.Cost,
                            Location = @event.Event.Location,
                            Description = @event.Event.Description,
                            ERepeat = @event.EventRepeat.ERepeat
                        };

                        await CreateIndividualEventAsync(weeklyEvent, userId, groupId, weeklyEventRepeat.EventRepeatId);
                        repeatDate = repeatDate.AddDays(7);
                    }

                    var weeklyGroupRepeatedEvent = new GroupRepeatedEvent
                    {
                        EventRepeatId = weeklyEventRepeat.EventRepeatId,
                        GroupId = groupId
                    };

                    _context.GroupRepeatedEvents.Add(weeklyGroupRepeatedEvent);
                    await _context.SaveChangesAsync();

                    break;

                case Repeat.Monthly:
                    endDate = repeatDate.AddYears(5);

                    var monthlyEventRepeat = new EventRepeat()
                    {
                        EventName = @event.Event.EventName,
                        ERepeat = @event.EventRepeat.ERepeat,
                        Location = @event.Event.Location,
                        Description = @event.Event.Description,
                        DayOfMonth = @event.Event.Start.Day,
                        FirstEventDate = @event.Event.Start,
                        EndEventDate = endDate
                    };
                    _context.EventRepeats.Add(monthlyEventRepeat);
                    await _context.SaveChangesAsync();
                    await CreateInvites(monthlyEventRepeat, groupId, userId);

                    while (repeatDate < endDate)
                    {
                        var monthlyEvent = new Event()
                        {
                            EventName = @event.Event.EventName,
                            Start = repeatDate,
                            Food = @event.Event.Food,
                            Cost = @event.Event.Cost,
                            Location = @event.Event.Location,
                            Description = @event.Event.Description,
                            ERepeat = @event.EventRepeat.ERepeat
                        };

                        await CreateIndividualEventAsync(monthlyEvent, userId, groupId, monthlyEventRepeat.EventRepeatId);
                        repeatDate = repeatDate.AddMonths(1);
                    }

                    var monthlyGroupRepeatedEvent = new GroupRepeatedEvent
                    {
                        EventRepeatId = monthlyEventRepeat.EventRepeatId,
                        GroupId = groupId
                    };

                    _context.GroupRepeatedEvents.Add(monthlyGroupRepeatedEvent);
                    await _context.SaveChangesAsync();

                    break;

                case Repeat.Yearly:
                    endDate = repeatDate.AddYears(100);

                    var annualEventRepeat = new EventRepeat()
                    {
                        EventName = @event.Event.EventName,
                        ERepeat = @event.EventRepeat.ERepeat,
                        Location = @event.Event.Location,
                        Description = @event.Event.Description,
                        DayOfMonth = @event.Event.Start.Day,
                        MonthOfYear = (MonthOfYear)@event.Event.Start.Month,
                        FirstEventDate = @event.Event.Start,
                        EndEventDate = endDate
                    };
                    _context.EventRepeats.Add(annualEventRepeat);
                    await _context.SaveChangesAsync();
                    await CreateInvites(annualEventRepeat, groupId, userId);

                    while (repeatDate < endDate)
                    {
                        var annualEvent = new Event()
                        {
                            EventName = @event.Event.EventName,
                            Start = repeatDate,
                            Food = @event.Event.Food,
                            Cost = @event.Event.Cost,
                            Location = @event.Event.Location,
                            Description = @event.Event.Description,
                            ERepeat = @event.EventRepeat.ERepeat
                        };

                        await CreateIndividualEventAsync(annualEvent, userId, groupId, annualEventRepeat.EventRepeatId);
                        repeatDate = repeatDate.AddYears(1);
                    }

                    var annualGroupRepeatedEvent = new GroupRepeatedEvent
                    {
                        EventRepeatId = annualEventRepeat.EventRepeatId,
                        GroupId = groupId
                    };

                    _context.GroupRepeatedEvents.Add(annualGroupRepeatedEvent);
                    await _context.SaveChangesAsync();

                    break;

                case Repeat.Once:
                    var onceEventRepeat = new EventRepeat()
                    {
                        EventName = @event.Event.EventName,
                        ERepeat = @event.EventRepeat.ERepeat,
                        Location = @event.Event.Location,
                        Description = @event.Event.Description,
                        FirstEventDate = @event.Event.Start,
                        EndEventDate = @event.Event.Start
                    };
                    _context.EventRepeats.Add(onceEventRepeat);
                    await _context.SaveChangesAsync();
                    await CreateInvites(onceEventRepeat, groupId, userId);

                    var IndividualEvent = new Event()
                    {
                        EventName = @event.Event.EventName,
                        Start = repeatDate,
                        Food = @event.Event.Food,
                        Cost = @event.Event.Cost,
                        Location = @event.Event.Location,
                        Description = @event.Event.Description,
                        ERepeat = @event.EventRepeat.ERepeat
                    };

                    await CreateIndividualEventAsync(IndividualEvent, userId, groupId, onceEventRepeat.EventRepeatId);

                    var onceGroupRepeatedEvent = new GroupRepeatedEvent
                    {
                        EventRepeatId = onceEventRepeat.EventRepeatId,
                        GroupId = groupId
                    };

                    _context.GroupRepeatedEvents.Add(onceGroupRepeatedEvent);
                    await _context.SaveChangesAsync();

                    break;

                default:
                    // TODO ===== need to throw this eventually
                    break;
            }
        }

        public async Task<bool> RespondToEventInvitation(long userId, long repeatedEventId, RSVPStatus rsvp)
        {
            Console.WriteLine("Got this far");
            EventInvite eventInvite = await _context.EventInvites.FindAsync(userId, repeatedEventId);
            eventInvite.Status = rsvp;

            _context.Entry(eventInvite).State = EntityState.Modified;
            Console.WriteLine("Got here sucker");

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!await EventInviteExists(userId, repeatedEventId, eventInvite))
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

        private async Task<bool> EventInviteExists(long userId, long repeatedEventId, EventInvite eventInvite)
        {
            return await _context.EventInvites.AnyAsync(ei => ei.EventRepeatId == repeatedEventId && ei.UserId == userId);
        }

        public async Task CreateIndividualEventAsync(Event @event, long userId, long groupId, long repeatId)
        {
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var repeatedEvent = new RepeatedEvent()
            {
                EventId = @event.EventId,
                EventRepeatId = repeatId
            };

            await _context.RepeatedEvents.AddAsync(repeatedEvent);
            await _context.SaveChangesAsync();

            @event.EventHost = new HostedEvent
            {
                UserId = userId,
                EventId = @event.EventId
            };

            await UpdateIndividualEventAsync(groupId, @event);
            await AddEventAsync(groupId, @event);
        }

        public async Task<long> FindUserIdByUserName(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            return user.Id;
        }

        public bool HostMatchesCurrent(long groupId, long current, HostedEvent host)
        {
            return current == host.UserId;
        }

        public async Task<bool> HostMatchesCurrentById(long groupId, long current, long eventId)
        {
            var hostedEvent = await _context.HostedEvents.FirstOrDefaultAsync(he => he.EventId == eventId);

            return HostMatchesCurrent(groupId, current, hostedEvent);
        }

        public async Task<GroupUser> GetGroupUser(long groupId, long userId)
        {
            return await _context.GroupUsers.FindAsync(groupId, userId);
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
            JoinRequest joinRequest = new JoinRequest
            {
                GroupId = groupId,
                UserId = userId
            };

            _context.JoinRequests.Add(joinRequest);
            await _context.SaveChangesAsync();
        }

        public async Task RespondToGroupJoinRequest(long groupId, long userId, JoinStatus status)
        {
            if (status == JoinStatus.accepted)
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

        public IEnumerable<GroupEventDto> GetAllCalendar(Repeat repeat, long groupId, long userId)
        {
            List<GroupEventDto> events = new List<GroupEventDto>();

            List<GroupEventDto> groupEvents = FindAllGroupEvents(groupId, userId).ToList();
            events.AddRange(
                groupEvents.Where(ge => ge.ERepeat == repeat)
            );

            return events;
        }

        public async Task<IEnumerable<GroupEventDto>> GetAllCalendar(Repeat repeat, long userId)
        {
            List<GroupEventDto> events = new List<GroupEventDto>();

            IEnumerable<long> groups = await GetGroupIds(userId);

            foreach (long id in groups)
            {
                List<GroupEventDto> groupEvents = FindAllGroupEvents(id, userId).ToList();
                events.AddRange(
                    groupEvents.Where(ge => ge.ERepeat == repeat)
                );
            }

            return events;
        }

        public IEnumerable<GroupEventDto> GetAllCalendar(long groupId, long userId)
        {
            return FindAllGroupEvents(groupId, userId);
        }

        public async Task<IEnumerable<GroupEventDto>> GetAllCalendar(long userId)
        {
            List<GroupEventDto> events = new List<GroupEventDto>();

            IEnumerable<long> groups = await GetGroupIds(userId);

            foreach (long id in groups)
            {
                GroupUser groupUser = await guRepo.GetGroupUser(id, userId);
                List<GroupEventDto> groupEvents = FindAllGroupEvents(id, userId).ToList();
                events.AddRange(groupEvents);
            }

            return events;
        }

        private async Task<IEnumerable<long>> GetGroupIds(long userId)
        {
            return await _context.Groups
                .Where(g => g.GroupUsers.Any(u => u.UserId == userId))
                .Select(g => g.GroupId)
                .ToListAsync();
        }
    }

    public interface IGroup
    {
        IEnumerable<GroupDto> GetAll(long userId);

        GroupDto Find(long id, long userId, GroupUser currentUser);
        Task<Group> GetGroup(long id);
        Task<bool> UpdateIndividualEventAsync(long groupId, Event @event);
        Task<bool> UpdateRepeatedEventAsync(long groupId, RepeatedEvent @event);
        Task<bool> UpdateDBRepeatedEventAsync(long groupId, EventRepeat @event);
        Task CreateAsync(Group group, long userId);

        Task<string> DeleteAsync(long id, long userId);

        Task<bool> UpdateAsync(Group group, long userId);

        Task AddEventAsync(long groupId, Event @event);

        Task<bool> DeleteRepeatedEventAsync(long groupId, long eventId);
        Task<bool> DeleteIndividualEventAsync(long groupId, long eventId);
        Task AddUserAsync(long groupId, string userName);
        Task AddUserAsync(long groupId, string username, Role newRole);
        Task<bool> RemoveUserAsync(GroupUser current, GroupUser adjusted);

        Task CreateInvites(EventRepeat eventRepeat, long groupId, long userId);
        Task CreateEventAsync(RepeatedEvent repeatEvent, long userId, long groupId);
        Task<bool> RespondToEventInvitation(long groupId, long repeatedEventId, RSVPStatus rsvp);
        Task CreateIndividualEventAsync(Event @event, long userId, long groupId, long repeatId);
        Task<long> FindUserIdByUserName(string userName);

        bool HostMatchesCurrent(long groupId, long current, HostedEvent host);
        Task<bool> HostMatchesCurrentById(long groupId, long current, long eventId);
        Task<GroupUser> GetGroupUser(long groupId, long userId);
        Task<IEnumerable<GroupDto>> SearchGroupsByString(string searchFor);
        Task RequestToJoinGroupById(long groupId, long userId);
        Task RespondToGroupJoinRequest(long groupId, long userId, JoinStatus status);
        IEnumerable<GroupEventDto> GetAllCalendar(Repeat repeat, long groupId, long userId);
        Task<IEnumerable<GroupEventDto>> GetAllCalendar(Repeat repeat, long userId);
        IEnumerable<GroupEventDto> GetAllCalendar(long groupId, long userId);
        Task<IEnumerable<GroupEventDto>> GetAllCalendar(long userId);
    }
}
