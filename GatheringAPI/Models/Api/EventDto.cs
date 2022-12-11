using System;
using System.Collections.Generic;

namespace GatheringAPI.Models.Api
{
    public class EventDto
    {
        public long EventId { get; set; }

        public string EventName { get; set; }

        public DateTime Start { get; set; }

        public DateTime? End { get; set; }

        public int DayOfMonth { get; set; }

        public decimal Cost { get; set; }

        public string Location { get; set; }

        public List<AttendingDto> Attending { get; set; }

        public string HostedBy { get; set; }

        public List<CommentDto> Comments { get; set; }
    }
}
