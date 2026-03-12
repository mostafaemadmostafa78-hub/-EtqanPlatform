using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class RegisterByPhoneDto
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
