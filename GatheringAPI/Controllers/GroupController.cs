using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Mvc;
using GatheringAPI.Models;
using GatheringAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        //private readonly GatheringDbContext _context;
        private readonly IGroup repository;
        private readonly IEvent eventRepo;
        private readonly IGroupUser guRepo;

        public GroupController(IGroup groupRepository, IEvent eventRepository, IGroupUser groupUser)
        {
            repository = groupRepository;
            eventRepo = eventRepository;
            guRepo = groupUser;
        }

        // GET: api/Group
        [HttpGet]
        public IEnumerable<GroupDto> GetGroups()
        {
            long userId = UserId;
            return repository.GetAll(userId);
        }

        private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // GET: api/Group/5
        [HttpGet("{id}")]
        public async Task<GroupDto> GetGroup(long id)
        {
            GroupUser currentUser = await guRepo.GetGroupUser(id, UserId);
            long userId = UserId;
            return repository.Find(id, userId, currentUser);
        }

        // PUT: api/Group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(long id, Group @group)
        {
            if (id != @group.GroupId)
            {
                return BadRequest();
            }

            bool didUpdate = await repository.UpdateAsync(@group, UserId);

            if (!didUpdate)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Group
        [HttpPost]
        public async Task<ActionResult<Group>> PostGroup(Group @group)
        {
            await repository.CreateAsync(@group, UserId);
            return CreatedAtAction("GetGroup", new { id = @group.GroupId }, @group);
        }

        // DELETE: api/Group/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Group>> DeleteGroup(long id)
        {
            long userId = UserId;
            string didDelete = await repository.DeleteAsync(id, userId);

            if (didDelete == "null")
                return NotFound();
            else if (didDelete == "false")
                return Unauthorized("You must be the owner of this group to delete it.");
            else
                return await repository.GetGroup(id);
        }

        // POST: api/Group/5/Event/3
        [HttpPost("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> AddEvent(long groupId, long eventId)
        {
            Event @event = await eventRepo.GetOneByIdAsync(eventId);

            await repository.AddEventAsync(groupId, @event);
            repository.SendInvites(@event);


            return CreatedAtAction(nameof(AddEvent), new { groupId, eventId }, null);
        }

        // DELETE: api/Group/5/Event/3
        [HttpDelete("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> DeleteEvent(long groupId, long eventId)
        {
            bool didDelete = await repository.DeleteEventAsync(groupId, eventId, UserId);

            if (didDelete == true)
                return Ok();
            else
                return Unauthorized("Error: Only the creator of this event or a group admin can delete it.");
        }

        // PUT: api/Group/5/Repeated/3
        [HttpPut("{groupId}/Repeated/{eventId}")]
        public async Task<ActionResult> UpdateRepeatedEvent(long groupId, Event @event)
        {
            bool didUpdate = await repository.UpdateRepeatedEventAsync(groupId, @event, UserId);

            if (didUpdate == true)
                return Ok();
            else
                return Unauthorized("Error: Only the creator of this event or a group admin can update it.");
        }

        // PUT: api/Group/5/Event/3
        [HttpPut("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> UpdateIndividualEvent(long groupId, Event @event)
        {
            bool didUpdate = await repository.UpdateIndividualEventAsync(groupId, @event, UserId);

            if (didUpdate == true)
                return Ok();
            else
                return Unauthorized("Error: Only the creator of this event or a group admin can update it.");
        }

        // POST: api/Group/5/User/jonstruve
        [HttpPost("{groupId}/User/{userName}")]
        public async Task<ActionResult> AddUser(long groupId, string userName)
        {
            GroupUser currentUser = await guRepo.GetGroupUser(groupId, UserId);
            GroupDto currentGroup = await GetGroup(groupId);

            if (currentUser.Role == Role.owner || currentUser.Role == Role.admin)
            {
                Console.WriteLine($"{currentGroup.GroupUsers.Count}, {currentGroup.MaxUsers}");
                if(currentGroup.GroupUsers.Count < currentGroup.MaxUsers || currentGroup.MaxUsers == -1)
                {
                    await repository.AddUserAsync(groupId, userName);

                    long userId = await repository.FindUserIdByUserName(userName);
                    return CreatedAtAction(nameof(AddUser), new { groupId, userName }, null);
                }
                else
                {
                    return BadRequest("Cannot add users to this group. This group is currently full. Please upgrade to add more users.");
                }
            }
            else
            {
                return Unauthorized("Only admins and owners can add users to this group. If you find this to be a mistake, please talk with your group admins.");
            }
        }

        //POST: api/Group/5/Event
        [HttpPost("{groupId}/Event")]
        public async Task<ActionResult<Event>> AddEventToGroup(RepeatedEvent repeatEvent, long groupId)
        {
            GroupUser currentUser = await guRepo.GetGroupUser(groupId, UserId);
            GroupDto currentGroup = await GetGroup(groupId);
            
            if(currentUser.Role != Role.user)
            {
                if(currentGroup.GroupRepeatedEvents == null || currentGroup.GroupRepeatedEvents.Count < currentGroup.MaxEvents || currentGroup.MaxEvents == -1)
                {
                    await repository.CreateEventAsync(repeatEvent, UserId, groupId);
                    return Ok();
                }
                else
                {
                    return BadRequest("Your group has reached the maximum number of created events for the month. Please upgrade to create more.");
                }
            }
            return Unauthorized("Only certain users in your group can create events. Please talk to the group admins if you think that should be you.");
        }
        
        //DELETE: api/Group/5/User/2
        [HttpDelete("{groupId}/User/{userId}")]
        public async Task<ActionResult> RemoveUserFromGroup(long groupId, long userId)
        {
            GroupUser current = await guRepo.GetGroupUser(groupId, UserId);
            GroupUser adjusted = await guRepo.GetGroupUser(groupId, userId);

            await repository.RemoveUserAsync(current, adjusted);
            return Ok();
        }

        //GET: api/Group/Search/odysseus
        [HttpGet("Search/{searchFor}")]
        public async Task<IEnumerable<GroupDto>> SearchGroups(string searchFor)
        {
            return await repository.SearchGroupsByString(searchFor);
        }

        //POST: api/Group/5/Request
        [HttpPost("{groupId}/Request")]
        public async Task<ActionResult> RequestToJoinGroup(long groupId)
        {
            try
            {
                await repository.RequestToJoinGroupById(groupId, UserId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //POST: api/Group/5/User/5/Request/Accept
        [HttpPost("{groupId}/User/{userId}/Request/{status}")]
        public async Task<ActionResult> RequestToJoinGroupResponse(long groupId, long userId, JoinStatus status)
        {
            GroupUser currentUser = await guRepo.GetGroupUser(groupId, UserId);
            Role role = currentUser.Role;

            var userInGroup = guRepo.GetGroupUser(groupId, userId);
            if(userInGroup == null)
            {
                return BadRequest("That user is already in the group. Something went wrong.");
            }

            if(role == Role.owner || role == Role.admin)
            {
                await repository.RespondToGroupJoinRequest(groupId, userId, status);
                return Ok();
            }
            else
            {
                return Unauthorized("Only admins or owners can respond to join requests. Please talk to the groups admins if you believe this to be a mistake.");
            }
        }
    }
}
