using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models
{
    public class RepeatedEvent
    {
        public long EventRepeatId { get; set; }

        public EventRepeat EventRepeat { get; set; }

        public long EventId { get; set; }

        public Event Event { get; set; }
    }
}
