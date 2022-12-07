using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GatheringAPI.Models
{
    public class Group
    {
        public long GroupId { get; set; }

        [Required]
        public string GroupName { get; set; }

        public List<GroupUser> GroupUsers { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public List<GroupEvent> GroupEvents { get; set; }

        public List<GroupRepeatedEvent> GroupRepeatedEvents { get; set; }

        public List<JoinRequest> RequestsToJoin { get; set; }

        [Required]
        public GroupSizes GroupSize { get; set; }

        public long MaxUsers { get; set; }

        public long MaxEvents { get; set; }
    }

    public enum GroupSizes
    {
        free = 50,
        large = 500,
        infinite = 1001
    }
}
