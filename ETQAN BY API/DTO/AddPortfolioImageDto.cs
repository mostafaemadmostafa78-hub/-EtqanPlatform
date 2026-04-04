namespace ETQAN_BY_API.DTO
{
    public class AddPortfolioImageDto
    {
        public IFormFile Image { get; set; } // الملف الفعلي
        public string? Description { get; set; } // وصف اختياري
    }
}