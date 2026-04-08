//ah
namespace ETQAN_BY_API.DTO
{
    public class UpdateArtisanProfileDto
    {
        // --- البيانات الشخصية ---
        public string? Email { get; set; }           // 1. البريد الإلكتروني
        public string? PhoneNumber { get; set; }     // 2. رقم الهاتف
        public string? Governorate { get; set; }     // 3. المنطقة (المحافظة)
        public string? NewPassword { get; set; }     // 4. تغيير كلمة السر (إضافي)

        // --- المعلومات الأساسية ---
        public string? Bio { get; set; }             // 5. عن الحرفي
        public int ExperienceYears { get; set; }      // 6. سنوات الخبرة
        public List<string>? Services { get; set; }  // 7. الخدمات (قائمة)

        // --- معلومات العمل ---
        public string? ServiceArea { get; set; }     // 8. نطاق الخدمة
        public decimal StartingPrice { get; set; }    // 9. سعر الخدمة
        public string? WorkHours { get; set; }       // 10. مواعيد العمل
        public string? ResponseTime { get; set; }    // 11. وقت الاستجابة
        public bool IsEmergencyAvailable { get; set; } // 12. خدمة الطوارئ
    }
}
//.
