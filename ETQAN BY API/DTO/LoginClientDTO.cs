using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class LoginClientDTO
    {
        [Required(ErrorMessage = "UserName is required.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
