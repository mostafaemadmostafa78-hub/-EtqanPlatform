using ETQAN.API.Models.Enums;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }
        

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string ? CompanyName { get; set; }
        public string? CommercialRegister { get; set; }
        public UserType UserType { get; set; }

        public Client? Client { get; set; }
        public Artisan? Artisan { get; set; }
        public Company? Company { get; set; }
        public string Governorate { get; set; }

        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? ReviewsWritten { get; set; }
        public ICollection<ServiceRequest>? ServiceRequests { get; set; }
    }
}