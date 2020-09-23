using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Services;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        //private readonly GatheringDbContext _context;
        private readonly IGroup repository;

        public GroupController(IGroup groupRepository)
        {
            repository = groupRepository;
        }

        // GET: api/Group
        [HttpGet]
        public IEnumerable<GroupDto> GetGroups()
        {
            return repository.GetAll();
        }

        // GET: api/Group/5
        [HttpGet("{id}")]
        public GroupDto GetGroup(long id)
        {
            return repository.Find(id);
        }

        // PUT: api/Group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(long id, Group @group)
        {
            if (id != @group.GroupId)
            {
                return BadRequest();
            }

            bool didUpdate = await repository.UpdateAsync(@group);

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
            await repository.CreateAsync(@group);
            return CreatedAtAction("GetGroup", new { id = @group.GroupId }, @group);
        }

        // DELETE: api/Group/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Group>> DeleteGroup(long id)
        {
            var @group = await repository.DeleteAsync(id);

            if (@group == null)
            {
                return NotFound();
            }
            return @group;
        }

        // POST: api/Group/5/Event/3
        [HttpPost("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> AddEvent(long groupId, long eventId)
        {
            await repository.AddEventAsync(groupId, eventId);
            return CreatedAtAction(nameof(AddEvent), new { groupId, eventId }, null);
        }

        // DELETE: api/Group/5/Event/3
        [HttpDelete("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> DeleteEvent(long groupId, long eventId)
        {
            await repository.DeleteEventAsync(groupId, eventId);
            return Ok();
        }

        // PUT: api/Group/5/Event/3
        [HttpPut("{groupId}/Event/{eventId}")]
        public async Task<bool> UpdateEvent(long groupId, Event @event)
        {
            bool didUpdate = await repository.UpdateEventAsync(groupId, @event);
            return didUpdate;
        }
    }
}
