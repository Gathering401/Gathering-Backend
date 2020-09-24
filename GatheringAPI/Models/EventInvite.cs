namespace GatheringAPI.Models
{
    public class EventInvite
    {
        public long EventId { get; set; }
        public long UserId { get; set; }

        public Event Event { get; set; }
        public User User { get; set; }

        public RSVPStatus Status { get; set; }
    }

    public enum RSVPStatus
    {
        Accept,
        Decline,
        Maybe,
        Pending
    }
}