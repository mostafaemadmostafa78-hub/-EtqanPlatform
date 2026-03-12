using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(50)]
        public string CommercialRegister { get; set; }

        [Required]
        [StringLength(250)]
        public string Address { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}