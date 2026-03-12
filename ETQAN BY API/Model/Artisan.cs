using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

public class Artisan
{
    public int Id { get; set; }

    [Required]
    [Range(18, 60)]
    public int Age { get; set; }

    [Required]
    [StringLength(14)]
    public string NationalId { get; set; }

    [Required]
    public MaritalStatus MaritalStatus { get; set; }

    [Required]
    public int JobId { get; set; }

    public Job Job { get; set; }

    [Required]
    public string ApplicationUserId { get; set; }

    public ApplicationUser User { get; set; }
}