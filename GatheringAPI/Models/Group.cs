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

        public bool IsPublic { get; set; } = false;

        public List<JoinRequest> RequestsToJoin { get; set; }

        [Required]
        public GroupSizes GroupSize { get; set; }

        public long MaxUsers { get; set; }

        public long MaxEvents { get; set; }
    }

    public enum GroupSizes
    {
        free = 20,
        extraSmall = 50,
        small = 100,
        medium = 250,
        large = 1000,
        infinite = 1001
    }
}
