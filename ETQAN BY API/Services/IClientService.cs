using ETQAN_BY_API.Model.DTOs;
using System.Threading.Tasks;

namespace ETQAN_BY_API.Services
{
    //ah
    public interface IClientService
    {
        Task<bool> UpdateReviewAsync(int reviewId, string clientId, UpdateReviewDto dto);
        Task<bool> DeleteReviewAsync(int reviewId, string clientId);
        Task<bool> DeleteClientAccountAsync(string userId);
    }
    //.
}