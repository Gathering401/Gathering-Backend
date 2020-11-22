using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models.Api
{
    public class GroupUserDto
    {
        public long GroupId { get; set; }
        public long UserId { get; set; }
        public Group Group { get; set; }
        public User User { get; set; }
        public string RoleString { get; set; }
        public Role Role { get; set; }
    }
}
