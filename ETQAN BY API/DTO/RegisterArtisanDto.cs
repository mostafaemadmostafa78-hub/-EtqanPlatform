using System.ComponentModel.DataAnnotations;

public class RegisterArtisanDto
{
    [Required]
    public string Fullname { get; set; } 

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string BirthDate { get; set; }

    [Required]
    public string NationalId { get; set; }

    [Required]
    public string phoneNumber { get; set; }

    [Required]
    public int JobId { get; set; } 

    [Required]
    public int MaritalStatus { get; set; } // الحالة الاجتماعية

    [Required, MinLength(6)]
    public string Password { get; set; }

    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}