using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class ContactFormDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? MessageContent { get; set; }
        public string? Complaint { get; set; }

        public string? ArtisanId { get; set; }
        public int? CompanyId { get; set; }
    }
}