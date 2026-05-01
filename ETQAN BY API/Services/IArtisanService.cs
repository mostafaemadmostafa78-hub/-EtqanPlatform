using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using Microsoft.AspNetCore.Http;

namespace ETQAN_BY_API.Services
{
    public interface IArtisanService
    {
        // 1. التحديث الشامل (البيانات + الصور)
        // لاحظي إننا بنستخدم UpdateArtisanProfileDto اللي ضفنا فيه ملفات الصور
        Task<bool> UpdateArtisanProfileAsync(string userId, UpdateArtisanProfileDto dto);

        // 2. عرض البيانات
        Task<ArtisanDetailsDto> GetArtisanDetailsAsync(string userId);

        // 3. إدارة معرض الأعمال (Portfolio)
        Task<bool> AddImageToPortfolioAsync(string userId, AddPortfolioImageDto dto);
        Task<bool> DeleteImageFromPortfolioAsync(int imageId);

        // 4. إدارة الطلبات والفواتير
        Task<List<ArtisanOrderDto>> GetArtisanOrdersAsync(string artisanId, string? status = null);
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<bool> CreateInvoiceFromRequestAsync(IssueInvoiceDto dto);

        // 5. التقييمات والحساب
        Task<bool> AddOrUpdateArtisanReviewAsync(string clientId, ArtisanReviewCreateDto dto);
        Task<bool> DeleteArtisanAsync(string userId);

        // ملاحظة: مسحنا UpdateArtisanPicturesAsync لأنها أصبحت جزء من الميثود الأولى
    }
}