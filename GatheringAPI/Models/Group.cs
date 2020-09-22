using System.ComponentModel.DataAnnotations;

namespace GatheringAPI.Models
{
    public class Group
    {
        public long GroupId { get; set; }

        [Required]
        public string GroupName { get; set; }

        //[Required]
        //public List<GroupUser> GroupUsers { get; set; } 

        public string Location { get; set; }

        public string Description { get; set; }

        //public list<GroupEvent> GroupEvents { get; set; }

    }
}
