using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int? ArtisanId { get; set; }
        public Artisan? Artisan { get; set; }
        //ah
        public int? ServiceRequestId { get; set; }

        [ForeignKey("ServiceRequestId")]
        public ServiceRequest? ServiceRequest { get; set; }
        public int? ClientId { get; set; }
        public Client Client { get; set; }
        public int? CompanyId { get; set; } 
        public Company? Company { get; set; }
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; // تاريخ كتابة التقييم
        //.            
    }
}
