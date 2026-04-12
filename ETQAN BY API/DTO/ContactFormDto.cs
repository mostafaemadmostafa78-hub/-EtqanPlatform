using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class ContactFormDto
    {

        // بيانات اختيارية لو العميل مش مسجل دخول، لكن لو مسجل بناخدها من الـ Token
        public string? Name { get; set; }
        public string? Email { get; set; }

        public string? MessageContent { get; set; } // الخانة الكبيرة في شاشة "تواصل معنا"
        public string? Complaint { get; set; }  // الخانة اللي في الـ Popup (قدم شكوتك)

        public int? ArtisanId { get; set; }  // رقم الحرفي اللي بنشتكيه
        public int? ReviewId { get; set; }   // رقم التقييم لو بنشتكي من تعليق معين

    }
}     
    

