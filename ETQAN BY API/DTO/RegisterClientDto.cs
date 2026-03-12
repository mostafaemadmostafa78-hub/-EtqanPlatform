using System.ComponentModel.DataAnnotations;

public class RegisterClientDto
{
    [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
    public string Email { get; set; }

    [Required(ErrorMessage = "المحافظة مطلوبة")]
    public string Governorate { get; set; }

    [Required(ErrorMessage = "كلمة السر مطلوبة")]
    [MinLength(8, ErrorMessage = "يجب ألا تقل كلمة السر عن 8 أحرف")] // خليها 8 زي الرياكت
    public string Password { get; set; }

    [Required(ErrorMessage = "تأكيد كلمة السر مطلوب")]
    [Compare("Password", ErrorMessage = "كلمتا السر غير متطابقتين")]
    public string ConfirmPassword { get; set; }
}