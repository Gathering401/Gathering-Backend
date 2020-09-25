using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models.Api
{
    public class AttendingDto
    {
        public string Name { get; set; }
        public RSVPStatus Status { get; set; }
    }
}
