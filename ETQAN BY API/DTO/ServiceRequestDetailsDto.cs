namespace ETQAN_BY_API.DTO
{
    public class ServiceRequestDetailsDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusArabic { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;

        // تفاصيل الطلب
        public RequestInfoSection Details { get; set; } = new();

        // بيانات الحرفي 
        public ArtisanInfoSection? Artisan { get; set; }
    }

    public class RequestInfoSection
    {
        public string Date { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PriceStatus { get; set; } = string.Empty;
    }

    public class ArtisanInfoSection
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }
}
