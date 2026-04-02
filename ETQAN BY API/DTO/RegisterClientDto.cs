using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class RegisterClientDto
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^0(10|11|12|15)\d{8}$", ErrorMessage = "رقم الهاتف المصري غير صحيح")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        public string Governorate { get; set; }

        [Required(ErrorMessage = "كلمة السر مطلوبة")]
        [MinLength(8, ErrorMessage = "يجب ألا تقل كلمة السر عن 8 أحرف")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة السر مطلوب")]
        [Compare("Password", ErrorMessage = "كلمتا السر غير متطابقين")]
        public string ConfirmPassword { get; set; }
    }

   
    
}