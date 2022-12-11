namespace GatheringAPI.Models.Api
{
    public class UserDto
    {
        public long Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        public string Token { get; set; }

        public long UpcomingDaysOut { get; set; }
    }
}
