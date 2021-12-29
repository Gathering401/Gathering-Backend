using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GatheringAPI.Models;
using GatheringAPI.Services;
using System.Security.Claims;
using GatheringAPI.Models.Api;

namespace GatheringAPI.Controllers
{
    [Route("api/GroupUser")]
    [ApiController]
    [Authorize]
    public class GroupUserController : ControllerBase
    {
        private readonly IGroupUser repository;
        private readonly IGroup groupRepo;

        public GroupUserController(IGroupUser repo, IGroup groupRepository)
        {
            repository = repo;
            groupRepo = groupRepository;
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
                return Unauthorized("Must be a group admin or owner to adjust other user's roles.");
            if (role == Role.admin && (adjustedUser.Role == Role.admin || adjustedUser.Role == Role.owner))
                return Unauthorized("Admins cannot change the role of other admins.");

            bool didDelete = await groupRepo.RemoveUserAsync(currentUser, adjustedUser);
            if(!didDelete)
                return Unauthorized("That user does not currently exist in this group. Something must have gone wrong. We're working on it.");

            await groupRepo.AddUserAsync(groupId, adjustedUser.User.UserName, groupUser.Role);

            return Ok();
        }

        // GET: api/GroupUser/1
        [HttpGet("{groupId}")]
        public async Task<GroupUserDto> GetCurrentUser(long groupId)
        {
            return await repository.GetGroupUserDto(groupId, CurrentUserId);
        }

        // GET api/GroupUser/1/User/23
        [HttpGet("{groupId}/User/{userId}")]
        public async Task<bool> IsUserInGroup(long groupId, long userId)
        {
            return await repository.IsUserAddedToGroup(groupId, userId, CurrentUserId);
        }
    }
}
