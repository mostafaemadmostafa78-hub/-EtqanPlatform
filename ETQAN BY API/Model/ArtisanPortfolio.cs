using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.Model
{
    public class ArtisanPortfolio
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } // مسار صورة الشغل

        [StringLength(100)]
        public string? Description { get; set; } // وصف للصورة (اختياري)

        [Required]
        public int ArtisanId { get; set; }
        public Artisan Artisan { get; set; }
    }
}
