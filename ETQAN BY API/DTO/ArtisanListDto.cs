namespace ETQAN_BY_API.DTO
{
    public class ArtisanListDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string JobName { get; set; } 
        public decimal Price { get; set; }
        public double Rating { get; set; }
        public string ImageUrl { get; set; }
    }
    public class ArtisanDetailsDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string JobName { get; set; }
        public string Bio { get; set; }
        public int ExperienceYears { get; set; }
        public decimal StartingPrice { get; set; }
        public string ProfilePicture { get; set; }
        public string Governorate { get; set; }
        public double Rating { get; set; }

        // قائمة صور معرض الأعمال
        public List<string> PortfolioImages { get; set; }
    }
}