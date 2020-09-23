using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models
{
    public class GroupUser
    {
        public long GroupId { get; set; }
        public long UserId { get; set; }

        public Group Group { get; set; }
        public User User { get; set; }
    }
}
