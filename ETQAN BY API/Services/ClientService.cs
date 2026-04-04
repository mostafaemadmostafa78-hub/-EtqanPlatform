using ETQAN.API.Data;
using Microsoft.EntityFrameworkCore;
using ETQAN_BY_API.DTO;

namespace ETQAN_BY_API.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<bool> DeleteClientAccountAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}