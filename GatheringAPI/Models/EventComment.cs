namespace GatheringAPI.Models
{
    public class EventComment
    {
        public long UserId { get; set; }
        public long EventId { get; set; }
        public User User { get; set; }
        public Event Event { get; set; }
        public string Comment { get; set; }
    }
}