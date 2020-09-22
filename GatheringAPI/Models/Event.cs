using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatheringAPI.Models
{
    public class Event
    {
        public long EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        // Stretchy Goaly
        // [Required]
        // public Repeat EventRepeat { get; set; }

        [Required]
        [Column(TypeName = "DATE")]
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        // Stretchy Goaly
        // public DayOfWeek Weekday { get; set; }

        public int DayOfMonth { get; set; }

        // Stretchy Goaly
        // public Poll Poll { get; set; }

        [Required]
        public bool Food { get; set; }

        [Required]
        [Column(TypeName = "MONEY")]
        public decimal Cost { get; set; }

        [Required]
        public string Location { get; set; }

        // [Required]
        // public List<GroupEvent> InvitedGroups { get; set; }

        // [Required]
        // public List<EventInvites> Attending { get; set; }

        // Stretchy Goaly
        // public List<EventComments> Comments { get; set; }
    }
    public enum Repeat
    {
        Weekly,
        Monthly,
        Yearly,
        Once
    }
}
