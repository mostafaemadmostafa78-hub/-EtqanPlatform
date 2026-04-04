using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Http; 
namespace ETQAN_BY_API.Services
{
    public interface IArtisanService
    {
        // تحديث بيانات الحرفي
        Task<bool> UpdateArtisanAsync(string userId, ArtisanDetailsDto dto);

        // حذف الحرفي (مسح من جدول الحرفيين واليوزرز)
        Task<bool> DeleteArtisanAsync(string userId);

        Task<bool> AddImageToPortfolioAsync(string userId, AddPortfolioImageDto dto);
        Task<bool> DeleteImageFromPortfolioAsync(int imageId);
    }
}