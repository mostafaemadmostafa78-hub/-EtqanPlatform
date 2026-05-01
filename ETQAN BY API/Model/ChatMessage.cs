namespace ETQAN_BY_API.Model
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string? MessageContent { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool? IsImage { get; set; } = false;
        public bool IsRead { get; set; } = false; 
    }
}