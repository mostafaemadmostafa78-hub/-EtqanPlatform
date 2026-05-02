namespace ETQAN_BY_API.Model
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? ComplaintText { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? ArtisanId { get; set; }
        public int? CompanyId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}