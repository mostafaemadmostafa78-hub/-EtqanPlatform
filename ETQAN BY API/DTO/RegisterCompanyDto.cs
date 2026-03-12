using System.ComponentModel.DataAnnotations;

public class RegisterCompanyDto
{
    [Required]
    public string CompanyName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
    [Required]
    public IFormFile CommercialRegisterFile { get; set; }
    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}