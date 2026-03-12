using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class ReviewCreateDto
    {
        [Range(1, 5)]
        public decimal Rating { get; set; }

        [Required, StringLength(500)]
        public string Comment { get; set; }

        [Required]
        public int ArtisanId { get; set; }
    }

    public class ReviewReadOnlyDto
    {
        public int Id { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        public string ReviewerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}