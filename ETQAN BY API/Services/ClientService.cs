//ah
using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace ETQAN_BY_API.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; 

        
        public ClientService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }



        // 1. جلب بيانات البروفايل الأساسية
        public async Task<ClientProfileDto> GetClientInfoAsync(string userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return new ClientProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Governorate = user.Governorate ?? "غير محدد",
                ProfilePicture = user.ProfilePicture ?? "/images/default-user.png"
            };
        }

        // 2. جلب سجل الطلبات (History)
        public async Task<List<ClientHistoryDto>> GetClientHistoryAsync(string userId, string? status = null)
        {
            var query = _context.ServiceRequests
                .Include(r => r.Artisan).ThenInclude(a => a.User)
                .Include(r => r.Artisan).ThenInclude(a => a.Job)
                .Where(r => r.Client.ApplicationUserId == userId);

            if (!string.IsNullOrEmpty(status) && status != "الكل")
            {
                query = query.Where(r => r.Status.ToString() == status);
            }

            return await query
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new ClientHistoryDto
                {
                    RequestId = r.Id,
                    ArtisanName = r.Artisan != null ? r.Artisan.User.FullName : "جاري البحث..",
                    ArtisanPhone = r.Artisan != null ? r.Artisan.User.PhoneNumber : "غير متاح",
                    JobName = r.Artisan != null ? r.Artisan.Job.Name : r.ServiceName,
                    ArtisanImage = r.Artisan != null ? (r.Artisan.User.ProfilePicture ?? "/images/default.svg") : "/images/default.svg",
                    Status = r.Status.ToString()
                }).ToListAsync();
        }
        public async Task<bool> DeleteClientAccountAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId); // بنجيب اليوزر بالـ UserManager
            if (user == null) return false;

            // تحديث البيانات الأساسية
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.Governorate = dto.Governorate;

            // الجزء الخاص بتغيير كلمة السر بأمان
            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                // بنشيل الباسورد القديمة ونحط الجديدة متشفره بضغطة واحدة
                var removeResult = await _userManager.RemovePasswordAsync(user);
                if (removeResult.Succeeded)
                {
                    await _userManager.AddPasswordAsync(user, dto.NewPassword);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
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
        

        //  التقييمات اللي كتبها العميل 

        public async Task<List<ReviewReadOnlyDto>> GetMyReviewsAsync(string userId)
        {
            return await _context.Reviews
                .Include(r => r.Artisan).ThenInclude(a => a.User)
                .Where(r => r.ReviewerId == userId)
                .Select(r => new ReviewReadOnlyDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewerId = r.ReviewerId,
                    CreatedAt = r.CreatedAt,
                    ArtisanName = r.Artisan.User.FullName, 
                    ArtisanImage = r.Artisan.User.ProfilePicture ?? "/images/default.svg" 
                }).ToListAsync();
        }
    }
}