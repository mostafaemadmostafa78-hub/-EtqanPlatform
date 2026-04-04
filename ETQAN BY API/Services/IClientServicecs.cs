using ETQAN_BY_API.DTO;

namespace ETQAN_BY_API.Services
{
    public interface IClientService
    {

        Task<bool> DeleteClientAccountAsync(string userId);
    }
}