namespace ETQAN_BY_API.Model
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty; // العنوان أو موضوع الرسالة
        public string Content { get; set; } = string.Empty; // محتوى الرسالة
        public string? ComplaintText { get; set; } // لو كتب حاجة في خانة "اكتب شكوتك"
        public int? ArtisanId { get; set; } // لو الشكوى ضد حرفي معين
        public int? ReviewId { get; set; }  // لو الشكوى بخصوص تقييم محدد 
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false; // عشان الأدمن يعلم عليها كـ "تمت القراءة"

    }
}
