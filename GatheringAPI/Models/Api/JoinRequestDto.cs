using GatheringAPI.Models;

namespace GatheringAPI.Models.Api
{
    public class JoinRequestDto
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public JoinStatus Status { get; set; }
    }
}