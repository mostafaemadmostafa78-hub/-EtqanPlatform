using ETQAN.API.Models.Enums;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETQAN.API.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        [Required] 
        public string FullName { get; set; }
        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; }
        [Required]
        public string? Address { get; set; }

        [Required]
        public string? Governorate { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        // العميل
        [Required]
        public string ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        // الحرفي
        public string? ArtisanId { get; set; }

        [ForeignKey("ArtisanId")]
        public virtual Artisan Artisan { get; set; }
        //الشركة  
        public int? CompanyId { get; set; } 
        public Company? Company { get; set; }
        public Review? Review { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }
}