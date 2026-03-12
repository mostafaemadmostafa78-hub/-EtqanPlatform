using ETQAN.API.Models.Enums;

using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        // العميل
        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; }

        // الحرفي
        public int? ArtisanId { get; set; }
        public Artisan? Artisan { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }
}