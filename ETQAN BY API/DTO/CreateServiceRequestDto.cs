using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class CreateServiceRequestDto
    {
        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; }
        public string FullName { get; set; } 
        public string Address { get; set; }   
        public string Governorate { get; set; }

        [Required]
        public string ClientId { get; set; }
        [Required]
        public string ArtisanId { get; set; }
        public int? CompanyId { get; set; }
    }
}
