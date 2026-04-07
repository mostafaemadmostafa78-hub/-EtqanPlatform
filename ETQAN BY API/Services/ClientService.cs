//ah
using ETQAN.API.Data;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using Microsoft.EntityFrameworkCore;


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

       // تعديل التقييم
        public async Task<bool> UpdateReviewAsync(int reviewId, string clientId, UpdateReviewDto dto)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewerId == clientId);
            if (review == null) return false;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            return await _context.SaveChangesAsync() > 0;
        }

        //حذف التقييم
        public async Task<bool> DeleteReviewAsync(int reviewId, string clientId)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewerId == clientId);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            return await _context.SaveChangesAsync() > 0;
        }
        //.
    }
}