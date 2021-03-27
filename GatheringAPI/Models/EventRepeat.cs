using System;
using System.ComponentModel.DataAnnotations;

namespace GatheringAPI.Models
{
    public class EventRepeat
    {
        public long EventRepeatId { get; set; }

        [Required]
        public Repeat ERepeat { get; set; }

        public long EventId { get; set; }

        public Event Event { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public int DayOfMonth { get; set; }

        public MonthOfYear MonthOfYear { get; set; }

        public DateTime FirstEventDate { get; set; }

        public DateTime EndEventDate { get; set; }
    }

    public enum Repeat
    {
        Weekly,
        Monthly,
        Yearly,
        Once
    }

    public enum MonthOfYear
    {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }
}
