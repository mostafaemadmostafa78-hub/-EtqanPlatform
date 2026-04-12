//ah
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using Microsoft.AspNetCore.Http;

namespace ETQAN_BY_API.Services
{
    public interface IArtisanService
    {
        // 1. تحديث البروفايل   
        Task<bool> UpdateArtisanProfileAsync(string userId, UpdateArtisanProfileDto dto);

        // 2. عرض تفاصيل الحرفي
        Task<ArtisanDetailsDto> GetArtisanDetailsAsync(string userId);

        // 3. حذف الحرفي
        Task<bool> DeleteArtisanAsync(string userId);

        // 4. معرض الأعمال
        Task<bool> AddImageToPortfolioAsync(string userId, AddPortfolioImageDto dto);
        Task<bool> DeleteImageFromPortfolioAsync(int imageId);

        // 5. إدارة الطلبات

        Task<List<ArtisanOrderDto>> GetArtisanOrdersAsync(string artisanId, string? status = null);

        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);

        Task<bool> AddOrUpdateReviewAsync(string clientId, UpdateReviewDto dto);

       
    }
}
//.