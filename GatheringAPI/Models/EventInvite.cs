namespace GatheringAPI.Models
{
    public class EventInvite
    {
        public long EventRepeatId { get; set; }
        public long UserId { get; set; }

        public EventRepeat EventRepeat { get; set; }
        public User User { get; set; }

        public RSVPStatus Status { get; set; }
    }

    public enum RSVPStatus
    {
        Pending,
        Accepted,
        Declined,
        Maybe
    }
}
