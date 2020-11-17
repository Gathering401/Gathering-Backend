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

        public GroupController(IGroup groupRepository, IEvent eventRepository)
        {
            repository = groupRepository;
            eventRepo = eventRepository;
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
        public GroupDto GetGroup(long id)
        {
            long userId = UserId;
            return repository.Find(id, userId);
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

            await repository.AddEventAsync(groupId, eventId);
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

        // PUT: api/Group/5/Event/3
        [HttpPut("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> UpdateEvent(long groupId, Event @event)
        {
            bool didUpdate = await repository.UpdateEventAsync(groupId, @event, UserId);

            if (didUpdate == true)
                return Ok();
            else
                return Unauthorized("Error: Only the creator of this event or a group admin can update it.");
        }

        // POST: api/Group/5/User/jonstruve
        [HttpPost("{groupId}/User/{userName}")]
        public async Task<ActionResult> AddUser(long groupId, string userName)
        {
            await repository.AddUserAsync(groupId, userName);

            long userId = await repository.FindUserIdByUserName(userName);
            return CreatedAtAction(nameof(AddUser), new { groupId, userName }, null);
        }

        //POST: api/Group/5/Event
        [HttpPost("{groupId}/Event")]
        public async Task<ActionResult<Event>> AddEventToGroup(Event @event, long groupId)
        {
            await repository.CreateEventAsync(@event, UserId, groupId);
            return Ok();    
        }
        
        //DELETE: api/Group/5/User/2
        [HttpDelete("{groupId}/User/{userId}")]
        public async Task<ActionResult> RemoveUserFromGroup(long groupId, long userId)
        {
            await repository.RemoveUserAsync(groupId, userId);
            return Ok();
        }
    }
}
