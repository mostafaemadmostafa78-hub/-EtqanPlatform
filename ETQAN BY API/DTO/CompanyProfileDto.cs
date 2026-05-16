namespace ETQAN_BY_API.DTO
{
    public class CompanyProfileUpdateDto
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Governorate { get; set; }
        public string? Description { get; set; }
        public string? ServiceDetails { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ServiceIds { get; set; }
        public string? ServiceArea { get; set; }
        public string? WorkingHours { get; set; }
        public string? ResponseTime { get; set; }
        public string? EmergencyService { get; set; }
        public IFormFile? ProfilePictureFile { get; set; }
        public IFormFile? CoverPhotoFile { get; set; }
    }

    public class AddPortfolioImageCompanyDto
    {
        public IFormFile ImageFile { get; set; }
        public string? Description { get; set; }
    }
}