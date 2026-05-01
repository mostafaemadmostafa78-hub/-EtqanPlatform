using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class ArtisanListDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string JobName { get; set; } 
        public double Rating { get; set; }
        public string ImageUrl { get; set; }
    }
    public class ArtisanDetailsDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string BirthDate { get; set; }
        public string JobName { get; set; }
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public string ProfilePicture { get; set; }
        public string? CoverPicture { get; set; }
        public string Governorate { get; set; }
        public double Rating { get; set; }
        public string? WorkHours { get; set; }       // مواعيد العمل
        public string? ServiceArea { get; set; }     // نطاق الخدمة
        public string? ResponseTime { get; set; }    // سرعة الاستجابة
        public bool IsEmergencyAvailable { get; set; } // خدمة الطوارئ
        public int CompletedOrdersCount { get; set; }
        public DateTime JoinedDate { get; set; }

        // قائمة صور معرض الأعمال
        public List<string> PortfolioImages { get; set; }
    }
}