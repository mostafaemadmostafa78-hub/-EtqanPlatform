//ah
using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class UpdateArtisanProfileDto
    {
        // --- البيانات الشخصية ---
        public string? FullName { get; set; }            // 1. الاسم الكامل 
        public string? BirthDate { get; set; }         // 2. تاريخ الميلاد
        public int? MaritalStatus { get; set; }        // 3. الحالة الاجتماعية
        public string? Email { get; set; }           // 4. البريد الإلكتروني
        public string? PhoneNumber { get; set; }     // 5. رقم الهاتف
        public string? Governorate { get; set; }     // 6. المنطقة /المحافظة


        // --- المعلومات الأساسية ---
        public string? Bio { get; set; }             // 7. عن الحرفي
        public int? ExperienceYears { get; set; }      // 8. سنوات الخبرة
        public string? Services { get; set; }  //      9. الخدمات

        // --- معلومات العمل ---
        public string? ServiceArea { get; set; }     // 10. نطاق الخدمة
        public string? WorkHours { get; set; }       // 11. مواعيد العمل
        public string? ResponseTime { get; set; }    // 12. وقت الاستجابة
        public bool? IsEmergencyAvailable { get; set; } // 13. خدمة الطوارئ

        public IFormFile? ProfilePic { get; set; } 
        public IFormFile? CoverPic { get; set; }
    }
   
}
//.
