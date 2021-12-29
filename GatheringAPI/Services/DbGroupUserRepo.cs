using GatheringAPI.Data;
using GatheringAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatheringAPI.Models.Api;

namespace GatheringAPI.Services
{
    public class DbGroupUserRepo : IGroupUser
    {
        private readonly GatheringDbContext _context;

        public DbGroupUserRepo(GatheringDbContext context)
        {
            _context = context;
        }

        public async Task<GroupUser> GetGroupUser(long groupId, long userId)
        {
            try
            {
                GroupUser user = await _context.GroupUsers
                    .Include(gu => gu.User)
                    .Include(gu => gu.Group)
                    .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.UserId == userId);
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<GroupUserDto> GetGroupUserDto(long groupId, long userId)
        {
            try
            {
                return await _context.GroupUsers
                    .Select(gu => new GroupUserDto
                    {
                        UserId = gu.UserId,
                        GroupId = gu.GroupId,
                        User = gu.User,
                        Group = gu.Group,
                        RoleString = gu.Role.ToString(),
                        Role = gu.Role
                    })
                    .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.UserId == userId);
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> IsUserAddedToGroup(long groupId, long userId, long currentUser)
        {
            GroupUser current = await GetGroupUser(groupId, currentUser);
            if(current.Role == Role.owner || current.Role == Role.admin)
            {
                GroupUser groupUser = await _context.GroupUsers.FirstAsync(gu => gu.GroupId == groupId && gu.UserId == userId);

                return groupUser != null;
            }
            return Unauthorized();
        }
    }

    public interface IGroupUser
    {
        Task<GroupUser> GetGroupUser(long groupId, long userId);
        Task<GroupUserDto> GetGroupUserDto(long groupId, long userId);
        Task<bool> IsUserAddedToGroup(long groupId, long userId, long currentUser);
    }
}
