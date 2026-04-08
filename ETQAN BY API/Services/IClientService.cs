//ah
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using System.Threading.Tasks;

namespace ETQAN_BY_API.Services
{
    
    public interface IClientService
    {
        Task<bool> UpdateReviewAsync(int reviewId, string clientId, UpdateReviewDto dto);
        Task<List<ReviewReadOnlyDto>> GetMyReviewsAsync(string clientId);
        Task<bool> DeleteReviewAsync(int reviewId, string clientId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<List<ClientHistoryDto>> GetClientHistoryAsync(string userId, string? status = null);
        Task<bool> DeleteClientAccountAsync(string userId);


    }
    //.
}