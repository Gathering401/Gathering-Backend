using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatheringAPI.Models
{
    public class Event
    {
        public long EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        public DateTime Start { get; set; }

        public DateTime? End { get; set; }

        [Required]
        public bool Food { get; set; }

        [Required]
        [Column(TypeName = "MONEY")]
        public decimal Cost { get; set; }

        [Required]
        public string Location { get; set; }

        public List<GroupEvent> InvitedGroups { get; set; }

        public List<EventInvite> Attending { get; set; }

        public string Description { get; set; }

        public HostedEvent EventHost { get; set; }

        public List<EventComment> Comments { get; set; }

        public Repeat ERepeat { get; set; }
    }
}
