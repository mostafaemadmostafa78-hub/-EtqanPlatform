using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class LoginArtisanDTO
    {
      
            [Required]
        [EmailAddress]
            public string Email { get; set; }  // بدل Email

            [Required]
            public string Password { get; set; }
        
    }
}
