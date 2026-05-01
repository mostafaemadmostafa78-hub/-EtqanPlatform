using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "كلمة السر الجديدة وتأكيدها غير متطابقين")]
        public string ConfirmPassword { get; set; } 
    }
}

