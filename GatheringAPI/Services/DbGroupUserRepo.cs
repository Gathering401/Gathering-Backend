using GatheringAPI.Data;
using GatheringAPI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    }

    public interface IGroupUser
    {
        Task<GroupUser> GetGroupUser(long groupId, long userId);
    }
}
