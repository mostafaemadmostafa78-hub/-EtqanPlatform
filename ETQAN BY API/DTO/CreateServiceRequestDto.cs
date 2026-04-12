using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class CreateServiceRequestDto
    {
        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int ClientId { get; set; }
        [Required]
        public int ArtisanId { get; set; }
    }
}
