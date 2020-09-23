using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models
{
    public class GroupEvent
    {
        public long GroupId { get; set; }
        public long EventId { get; set; }

        // Navigation Property
        public Group Group { get; set; }
        public Event Event { get; set; }
    }
}
