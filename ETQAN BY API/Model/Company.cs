using ETQAN.API.Models;
using ETQAN_BY_API.Model;
using System.ComponentModel.DataAnnotations;

public class Company
{
    public int Id { get; set; }

    [Required]
    public string CompanyName { get; set; }
    [Required]
    [StringLength(500)]
    public string CommercialRegister { get; set; }
    [Required]
    [StringLength(250)]
    public string Governorate { get; set; }
    public string? Description { get; set; }
    public string? ServiceIds { get; set; }
    public int? ExperienceYears { get; set; }
    public string? WorkingHours { get; set; }
    public string? ServiceArea { get; set; }      
    public string? ResponseTime { get; set; }  
    public bool IsEmergencyAvailable { get; set; }
    public string? CoverPhoto { get; set; }
    public decimal AverageRating { get; set; } = 0;
    public ICollection<ServiceRequest> Requests { get; set; } = new List<ServiceRequest>();
    public string ApplicationUserId { get; set; }
    public ApplicationUser User { get; set; }
    public ICollection<CompanyPortfolio> Portfolio { get; set; }
    public ICollection<CompanyService> CompanyServices { get; set; }
    public ICollection<Review> Reviews { get; set; }
}