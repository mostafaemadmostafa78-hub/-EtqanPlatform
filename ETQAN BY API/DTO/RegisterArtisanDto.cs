using ETQAN.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class RegisterArtisanDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "العمر مطلوب")]
        [Range(18, 100, ErrorMessage = "العمر يجب أن يكون بين 18 و 100")]
        public int Age { get; set; }

        // ✅ int لأن React بيبعت ID رقمي
        [Required(ErrorMessage = "الحالة الاجتماعية مطلوبة")]
        public int MaritalStatus { get; set; }

        [Required(ErrorMessage = "الرقم القومي مطلوب")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "الرقم القومي يجب أن يكون 14 رقم")]
        public string NationalId { get; set; }

        // ✅ int لأن React بيبعت JobId رقمي
        [Required(ErrorMessage = "المهنة مطلوبة")]
        public int JobId { get; set; }

        [Required(ErrorMessage = "كلمة السر مطلوبة")]
        [MinLength(8, ErrorMessage = "كلمة السر يجب أن تكون 8 أحرف على الأقل")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة السر مطلوب")]
        [Compare("Password", ErrorMessage = "كلمة السر وتأكيدها غير متطابقتين")]
        public string ConfirmPassword { get; set; }
    }
}