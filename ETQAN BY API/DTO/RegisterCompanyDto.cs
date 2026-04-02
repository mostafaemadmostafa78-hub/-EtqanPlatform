using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ETQAN_BY_API.DTO
{
    public class RegisterCompanyDto
    {
        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "اسم الشركة يجب أن يكون بين 3 و 100 حرف")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^0(10|11|12|15)\d{8}$", ErrorMessage = "رقم الهاتف المصري غير صحيح")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "يرجى رفع ملف السجل التجاري")]
        public IFormFile CommercialRegisterFile { get; set; }

        [Required(ErrorMessage = "كلمة السر مطلوبة")]
        [MinLength(8, ErrorMessage = "يجب ألا تقل كلمة السر عن 8 أحرف")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة السر مطلوب")]
        [Compare("Password", ErrorMessage = "كلمتا السر غير متطابقتين")]
        public string ConfirmPassword { get; set; }
    }
}