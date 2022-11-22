using System;

namespace GatheringAPI.Models.Api
{
    public class EventInviteDto
    {
        public string EventName { get; set; }
        public long EventRepeatId { get; set; }
        public Repeat ERepeat { get; set; }
        public RSVPStatus Status { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int DayOfMonth { get; set; }
        public MonthOfYear MonthOfYear { get; set; }
        public DateTime FirstEventDate { get; set; }
    }
}
