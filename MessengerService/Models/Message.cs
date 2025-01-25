namespace MessengerService.Models
{
    public class Message
    {
        public Guid ID { get; set; }
        public string RecipientAddress { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? ReadTimestamp { get; set; }
        public DateTime SentTimestamp { get; set; }
    }
}
