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
    public class CalendarController : ControllerBase
    {
        private readonly IGroup repository;
        private readonly IEvent eventRepo;

        public CalendarController(IGroup Repository, IEvent EventRepo)
        {
            repository = Repository;
            eventRepo = EventRepo;
        }

        private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // GET: api/Calendar
        [HttpGet("Repeat/{repeat?}/Group/{groupId}")]
        public IEnumerable<GroupEventDto> GetByRepeatAndId(Repeat repeat, long groupId)
        {
            return repository.GetAllCalendar(repeat, groupId, UserId);
        }

        [HttpGet("Repeat/{repeat?}")]
        public async Task<IEnumerable<GroupEventDto>> GetByRepeat(Repeat repeat)
        {
            return await repository.GetAllCalendar(repeat, UserId);
        }

        [HttpGet("Group/{groupId}")]
        public IEnumerable<GroupEventDto> GetById(long groupId)
        {
            return repository.GetAllCalendar(groupId, UserId);
        }

        [HttpGet]
        public async Task<IEnumerable<GroupEventDto>> Get()
        {
            return await repository.GetAllCalendar(UserId);
        }

        [HttpGet("Invitations")]
        public IEnumerable<EventInviteDto> GetInvitationsByDate()
        {
            return eventRepo.GetInvitations(UserId);
        }

        //PUT: api/Group/5/Event/5/Invitation/1
        [HttpPut("Event/{repeatedEventId}/Invitation/{rsvp}")]
        public async Task<IActionResult> UpdateInvitationRSVP(long repeatedEventId, RSVPStatus rsvp)
        {
            await repository.RespondToEventInvitation(UserId, repeatedEventId, rsvp);

            bool didUpdate = await repository.RespondToEventInvitation(UserId, repeatedEventId, rsvp);

            if (!didUpdate)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}