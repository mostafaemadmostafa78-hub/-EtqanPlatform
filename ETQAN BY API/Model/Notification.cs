namespace ETQAN_BY_API.Model
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }      // المعرف بتاع المستخدم اللي هيستلم الإشعار
        public string Title { get; set; }       // عنوان الإشعار مثلاً: رسالة جديدة أو طلب خدمة
        public string Message { get; set; }     // نص الإشعار التفصيلي
        public string? ActionUrl { get; set; }  // رابط يودي المستخدم لصفحة معينة : الشات
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false; // عشان نعرف المستخدم شاف الإشعار ولا لسه
    }
}
