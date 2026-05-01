using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class ReviewCreateDto
    {
        [Range(1, 5)]
        public decimal Rating { get; set; }

        [Required, StringLength(500)]
        public string Comment { get; set; }

        public int OrderId { get; set; }
    }

    public class CompanyReviewCreateDto : ReviewCreateDto
    {
        public int CompanyId { get; set; }
    }

    public class ArtisanReviewCreateDto : ReviewCreateDto
    {
        public int ArtisanId { get; set; }
    }

    public class ReviewReadOnlyDto
    {
        public int Id { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        public string ReviewerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TargetName { get; set; }
        public string TargetImage { get; set; }
    }
}