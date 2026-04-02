using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.Model;
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

    // --- الإضافات الجديدة الاحترافية ---
    [Range(0, 100000)]
    public decimal StartingPrice { get; set; } // سعر الخدمة بيبدأ من كام؟

    public int ExperienceYears { get; set; } // سنين الخبرة

    [StringLength(500)]
    public string? Bio { get; set; } // نبذة تعريفية عن الحرفي

    // علاقة مع معرض الأعمال (الصور)
    public ICollection<ArtisanPortfolio>? Portfolio { get; set; }
}