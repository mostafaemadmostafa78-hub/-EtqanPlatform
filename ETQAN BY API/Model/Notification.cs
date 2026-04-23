namespace ETQAN_BY_API.Model
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? ActionUrl { get; set; }

        // نوع التنبيه (Chat أو Order) لتقسيم الجرس في الواجهة
        public string NotificationType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}