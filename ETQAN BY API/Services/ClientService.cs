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
        private readonly IFileService _fileService;

        public ClientService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<ClientProfileDisplayDto> GetClientInfoAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            return new ClientProfileDisplayDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Governorate = user.Governorate ?? "غير محدد",
                ProfilePictureUrl = user.ProfilePicture ?? "/images/default-user.png",
                CoverPictureUrl = user.CoverPicture ?? "/images/default-cover.png"
            };
        }

        public async Task<bool> UpdateProfileComprehensiveAsync(string userId, ClientProfileUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Governorate))
                user.Governorate = dto.Governorate;

            if (dto.ProfileFile != null)
            {
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                    _fileService.DeleteImage(user.ProfilePicture);

                user.ProfilePicture = await _fileService.UploadImageAsync(dto.ProfileFile, "uploads/profiles/clients");
            }

            if (dto.CoverPhotoFile != null)
            {
                if (!string.IsNullOrEmpty(user.CoverPicture)) 
                    _fileService.DeleteImage(user.CoverPicture);

                user.CoverPicture = await _fileService.UploadImageAsync(dto.CoverPhotoFile, "uploads/covers/clients");
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
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
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (client == null) return false;

            client.IsDeleted = true;

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        // تعديل التقييم
        public async Task<bool> UpdateReviewAsync(int reviewId, string clientId, UpdateReviewDto dto)
        {
            var review = await _context.Reviews
                .Include(r => r.Artisan)
                .Include(r => r.Company)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.ReviewerId == clientId);

            if (review == null) return false;

            if ((DateTime.Now - review.CreatedAt).TotalHours > 24)
            {
                throw new Exception("عذراً، لا يمكن تعديل التقييم بعد مرور 24 ساعة.");
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.CreatedAt = DateTime.Now;

            var saved = await _context.SaveChangesAsync() > 0;

            if (saved)
            {
                if (review.ArtisanId.HasValue)
                    await UpdateArtisanAverageRating(review.ArtisanId.Value);
                else if (review.CompanyId.HasValue)
                    await UpdateCompanyAverageRating(review.CompanyId.Value);
            }

            return saved;
        }

        private async Task UpdateArtisanAverageRating(int artisanId)
        {
            var artisan = await _context.Artisans.Include(a => a.Reviews).FirstOrDefaultAsync(a => a.Id == artisanId);
            if (artisan != null)
            {
                artisan.AverageRating = artisan.Reviews.Any()
                    ? (decimal)Math.Round(artisan.Reviews.Average(r => (double)r.Rating), 1)
                    : 0;
                await _context.SaveChangesAsync();
            }
        }

        private async Task UpdateCompanyAverageRating(int companyId)
        {
            var company = await _context.Companies.Include(c => c.Reviews).FirstOrDefaultAsync(c => c.Id == companyId);
            if (company != null)
            {
                company.AverageRating = company.Reviews.Any()
                    ? (decimal)Math.Round(company.Reviews.Average(r => (double)r.Rating), 1)
                    : 0;
                await _context.SaveChangesAsync();
            }
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
                    Rating = (decimal)Math.Round((double)r.Rating, 1),
                    Comment = r.Comment,
                    ReviewerId = r.ReviewerId,
                    CreatedAt = r.CreatedAt,
                    TargetName = r.Artisan.User.FullName, 
                    TargetImage = r.Artisan.User.ProfilePicture ?? "/images/default.svg" 
                }).ToListAsync();
        }
    }
}