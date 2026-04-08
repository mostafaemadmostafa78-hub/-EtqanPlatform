using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public decimal Rating { get; set; } = 0;

        [StringLength(500)]
        public string? Comment { get; set; }

        [Required]
        public string ReviewerId { get; set; }
        public ApplicationUser Reviewer { get; set; }

        [Required]
        public int ArtisanId { get; set; }
        public Artisan Artisan { get; set; }
        //ah
        public DateTime CreatedAt { get; set; } = DateTime.Now; // تاريخ كتابة التقييم
        //.            
    }
}
