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
        public IConfiguration Configuration { get; }

        private readonly GatheringDbContext _context;

        public DbGroupUserRepo(GatheringDbContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public async Task<GroupUser> GetGroupUser(long groupId, long userId)
        {
            try
            {
                GroupUser user = await _context.GroupUsers.FindAsync(groupId, userId);
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateUserRole(long groupId, long userId, GroupUser groupUser)
        {
            _context.Entry(groupUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!GroupUserExists(groupId, userId))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
            return true;
        }

        private bool GroupUserExists(long groupId, long userId)
        {
            return _context.GroupUsers.Any(gu => gu.GroupId == groupId && gu.UserId == userId);
        }
    }

    public interface IGroupUser
    {
        Task<GroupUser> GetGroupUser(long groupId, long userId);

        Task<bool> UpdateUserRole(long groupId, long userId, GroupUser groupUser);
    }
}
