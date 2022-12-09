using System;
using System.Collections.Generic;

namespace GatheringAPI.Models.Api
{
    public class EventDto
    {
        public long EventId { get; set; }
        //[Required]
        public string EventName { get; set; }

        //[Required]
        public DateTime Start { get; set; }

        public DateTime? End { get; set; }

        public int DayOfMonth { get; set; }

        //[Required]
        public decimal Cost { get; set; }

        //[Required]
        public string Location { get; set; }

        public List<AttendingDto> Attending { get; set; }

        public string HostedBy { get; set; }

        public List<CommentDto> Comments { get; set; }
    }
}
