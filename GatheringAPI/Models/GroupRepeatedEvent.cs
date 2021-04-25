using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models
{
    public class GroupRepeatedEvent
    {
        public long GroupId { get; set; }
        public long EventRepeatId { get; set; }

        public Group Group { get; set; }
        public EventRepeat EventRepeat { get; set; }
    }
}
