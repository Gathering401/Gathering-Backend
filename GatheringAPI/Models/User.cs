using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models
{
    public class User : IdentityUser<long>
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime BirthDate { get; set; }

        public List<EventInvite> Invites { get; set; }

        public List<HostedEvent> HostedEvents { get; set; }

        public long UpcomingDaysOut { get; set; } = 30;
    }
}
