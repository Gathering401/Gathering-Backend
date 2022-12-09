using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GatheringAPI.Models;
using GatheringAPI.Services;
using Microsoft.Extensions.Configuration;
using GatheringAPI.Models.Api;
using System.Security.Claims;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEvent repository;
        private readonly IGroup groupRepo;
        public IConfiguration Configuration { get; }

        public EventController(IEvent repository, IGroup groupRepo, IConfiguration configuration)
        {
            this.repository = repository;
            this.groupRepo = groupRepo;
            Configuration = configuration;
        }
        private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // GET: api/Event
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            return await repository.GetAllAsync();
        }

        // GET: api/Event/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(long id)
        {
            var @event = await repository.GetOneByIdAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // PUT: api/Event/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(long id, Event @event)
        {
            if (id != @event.EventId)
            {
                return BadRequest();
            }

            bool DidUpdate = await repository.UpdateByIdAsync(@event);

            if (!DidUpdate)
                return NotFound();

            return NoContent();
        }

        // POST: api/Event
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            await repository.CreateEventAsync(@event, UserId);
            return CreatedAtAction("GetEvent", new { id = @event.EventId }, @event);
        }

        // DELETE: api/Event/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEvent(long id)
        {
            var @event = await repository.DeleteAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // GET: api/Event/Upcoming/30
        [HttpGet("Upcoming/{daysOut}")]
        public async Task<IEnumerable<UpcomingEventDto>> getUpcoming(long daysOut)
        {
            return await groupRepo.GetUpcomingEvents(daysOut, UserId);
        }
    }
}
