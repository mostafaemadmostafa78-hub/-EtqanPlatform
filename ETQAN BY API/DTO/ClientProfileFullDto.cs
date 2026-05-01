namespace ETQAN_BY_API.DTO
{
    // بيانات البروفايل الأساسية
    public class ClientProfileDisplayDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Governorate { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? CoverPictureUrl { get; set; }
    }

    public class ClientProfileUpdateDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string ?PhoneNumber { get; set; }
        public string? Governorate { get; set; }
        public IFormFile? ProfileFile { get; set; }
        public IFormFile? CoverPhotoFile { get; set; }
    }

    // بيانات سجل الطلبات (History)
    public class ClientHistoryDto
    {
        public int RequestId { get; set; }
        public string ArtisanName { get; set; }
        public string ArtisanPhone { get; set; }
        public string JobName { get; set; }
        public string ArtisanImage { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
    }

    // التقييمات التي كتبها العميل
    public class ClientReviewDto
    {
        public int ReviewId { get; set; }
        public string ArtisanName { get; set; }
        public string ArtisanJob { get; set; }
        public string ArtisanImage { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
    }

    // نموذج تحديث البيانات
    //public class UpdateProfileDto
    //{
    //}
}