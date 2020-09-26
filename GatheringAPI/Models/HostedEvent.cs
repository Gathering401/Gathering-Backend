namespace GatheringAPI.Models
{
    public class HostedEvent
    {
        public long EventId { get; set; }
        public long UserId { get; set; }

        public Event Event { get; set; }
        public User User { get; set; }
    }
}