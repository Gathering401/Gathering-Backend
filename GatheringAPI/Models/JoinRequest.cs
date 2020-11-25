using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models
{
    public class JoinRequest
    {
        public long UserId { get; set; }

        public long GroupId { get; set; }

        public User User { get; set; }
        
        public Group Group { get; set; }

        public JoinStatus Status { get; set; }
    }

    public enum JoinStatus
    {
        requested,
        declined,
        accepted
    }
}
