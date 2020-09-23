using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GatheringAPI.Data;
using GatheringAPI.Models;
using GatheringAPI.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly GatheringDbContext _context;
        private readonly IEvent repository;
        public IConfiguration Configuration { get; }

        public EventController(GatheringDbContext context, IEvent repository, IConfiguration configuration)
        {
            this.repository = repository;
            _context = context;
            Configuration = configuration;
        }

        // GET: api/Event
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
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
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            SendInvites(@event);

            return CreatedAtAction("GetEvent", new { id = @event.EventId }, @event);
        }

        public string _accountSid = null;
        public string _authToken = null;
        public string _phone = null;

        private void SendInvites(Event @event)
        {
            _phone = Configuration["Twilio:phone"];
            _accountSid = Configuration["Twilio:accountSid"];
            _authToken = Configuration["Twilio:authToken"];

            TwilioClient.Init(_accountSid, _authToken);

            foreach (var group in @event.InvitedGroups)
            {
                foreach (var user in group.Group.GroupUsers)
                {
                    var message = MessageResource.Create(
                        body: $"You've been invited to {@event.EventName}! Please reply with your RSVP - 1 for Yes, 2 for No, 3 for Maybe. Your response will apply to the most recent invitation without a response.",
                        from: new Twilio.Types.PhoneNumber($"+1{_phone}"),
                        to: new Twilio.Types.PhoneNumber($"+1{user.User.PhoneNumber}")
                        );

                    Console.WriteLine(message.Sid);
                }
            }
        }

        // DELETE: api/Event/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEvent(long id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return @event;
        }

        private bool EventExists(long id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
