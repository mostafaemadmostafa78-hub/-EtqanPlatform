using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.Model;
using System.ComponentModel.DataAnnotations;

public class Artisan
{
    public int Id { get; set; }

    [Required]
    public string BirthDate { get; set; }      

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
    //ah
    // ---الإضافات الجديدة  ---
    [Range(0, 100000)]
    public decimal StartingPrice { get; set; } // سعر الخدمة بيبدأ من كام؟

    public int ExperienceYears { get; set; } // سنين الخبرة

    [StringLength(500)]
    public string? Bio { get; set; } // نبذة تعريفية عن الحرفي

    [StringLength(100)]
    public string? WorkHours { get; set; } // مواعيد العمل
    public string? Services { get; set; } // الخدمات

    [StringLength(200)]
    public string? ServiceArea { get; set; } // نطاق الخدمة (المنطقة)

    [StringLength(50)]
    public string? ResponseTime { get; set; } // سرعة الاستجابة

    public bool IsEmergencyAvailable { get; set; } // متاح للطوارئ؟
    public decimal AverageRating { get; set; } = 0;

    [StringLength(255)]
    public string? CoverPicture { get; set; }

    // علاقة مع معرض الأعمال
    public ICollection<ArtisanPortfolio> Portfolio { get; set; } = new List<ArtisanPortfolio>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    //.
}