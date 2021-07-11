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

        public CalendarController(IGroup Repository)
        {
            repository = Repository;
        }

        private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // GET: api/Calendar
        [HttpGet("Repeat/{repeat?}/Group/{groupId}")]
        public async Task<IEnumerable<GroupEventDto>> GetByRepeatAndId(Repeat repeat, long groupId)
        {
            return await repository.GetAllCalendar(repeat, groupId, UserId);
        }

        [HttpGet("Repeat/{repeat?}")]
        public async Task<IEnumerable<GroupEventDto>> GetByRepeat(Repeat repeat)
        {
            return await repository.GetAllCalendar(repeat, UserId);
        }

        [HttpGet("Group/{groupId}")]
        public async Task<IEnumerable<GroupEventDto>> GetById(long groupId)
        {
            return await repository.GetAllCalendar(groupId, UserId);
        }

        [HttpGet]
        public async Task<IEnumerable<GroupEventDto>> Get()
        {
            return await repository.GetAllCalendar(UserId);
        }
    }
}