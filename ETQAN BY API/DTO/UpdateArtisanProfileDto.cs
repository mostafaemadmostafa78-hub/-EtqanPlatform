//ah
namespace ETQAN_BY_API.DTO
{
    public class UpdateArtisanProfileDto
    {
        // --- البيانات الشخصية ---
        public string FullName { get; set; }            // 1. الاسم الكامل 
        public int Age { get; set; }                   // 2. العمر
        public int MaritalStatus { get; set; }        // 3. الحالة الاجتماعية
        public string? Email { get; set; }           // 4. البريد الإلكتروني
        public string? PhoneNumber { get; set; }     // 5. رقم الهاتف
        public string? Governorate { get; set; }     // 6. المنطقة /المحافظة


        // --- المعلومات الأساسية ---
        public string? Bio { get; set; }             // 7. عن الحرفي
        public int ExperienceYears { get; set; }      // 8. سنوات الخبرة
        public string? Services { get; set; }  // 9. الخدمات (قائمة)

        // --- معلومات العمل ---
        public string? ServiceArea { get; set; }     // 10. نطاق الخدمة
        public decimal StartingPrice { get; set; }    // 11. سعر الخدمة
        public string? WorkHours { get; set; }       // 12. مواعيد العمل
        public string? ResponseTime { get; set; }    // 13. وقت الاستجابة
        public bool IsEmergencyAvailable { get; set; } // 14. خدمة الطوارئ

        public string? CurrentPassword { get; set; }    // 15. كلمة السر الحالية 
        public string? NewPassword { get; set; }     // 16. كلمة السر الجديدة
        public string? ConfirmPassword { get; set; }  // 17. تأكيد كلمة السر الجديدة
    }
}
//.
