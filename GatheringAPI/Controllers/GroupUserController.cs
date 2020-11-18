using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GatheringAPI.Models;
using GatheringAPI.Services;
using System.Security.Claims;

namespace GatheringAPI.Controllers
{
    [Route("api/GroupUser")]
    [ApiController]
    [Authorize]
    public class GroupUserController : ControllerBase
    {
        private readonly IGroupUser repository;

        public GroupUserController(IGroupUser repo)
        {
            repository = repo;
        }

        private long CurrentUserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // PUT: api/GroupUser/1/User/3
        [HttpPut("{groupId}/User/{userId}")]
        public async Task<ActionResult> ChangeUserRole(long groupId, long userId, GroupUser groupUser)
        {
            GroupUser currentUser = await repository.GetGroupUser(groupId, CurrentUserId);
            GroupUser adjustedUser = await repository.GetGroupUser(groupId, userId);
            Role role = currentUser.Role;
            if(role == Role.user || role == Role.creator)
            {
                return Unauthorized("Must be group admin or owner to adjust user's roles.");
            }
            if (role == Role.admin && (adjustedUser.Role == Role.admin || adjustedUser.Role == Role.owner))
            {
                return Unauthorized("Admin cannot change role of other admins.");
            }
            bool didUpdate = await repository.UpdateUserRole(groupId, userId, groupUser);
            if (didUpdate)
            {
                return Ok();
            }
            else
            {
                return Unauthorized("That user does not currently exist in this group. Something must have gone wrong. We're working on it.");
            }
        }
    }
}
