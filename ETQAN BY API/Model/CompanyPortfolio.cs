using System.ComponentModel.DataAnnotations;
using ETQAN.API.Models;

namespace ETQAN_BY_API.Model
{
    public class CompanyPortfolio
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [Required]
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}