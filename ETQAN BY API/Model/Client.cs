using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }
        //public  string ? Government { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<ServiceRequest>? Requests { get; set; }
    }
}