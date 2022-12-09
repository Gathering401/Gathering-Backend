using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models.Api
{
    public class UpcomingEventDto
    {
        public long EventId { get; set; }

        public string EventName { get; set; }

        public DateTime Start { get; set; }

        public string DaysFromNow { get; set; }
    }
}
