using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<ServiceRequest>? Requests { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}