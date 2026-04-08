//ah
using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model;
using ETQAN_BY_API.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ETQAN_BY_API.Services
{
    public class ArtisanService : IArtisanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ArtisanService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // 1. عرض تفاصيل الحرفي
        public async Task<ArtisanDetailsDto> GetArtisanDetailsAsync(string userId)
        {
            var artisan = await _context.Artisans
                .Include(a => a.User)
                .Include(a => a.Job)
                .Include(a => a.Portfolio)
                .Include(a => a.Reviews)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return null;

            return new ArtisanDetailsDto
            {
                Id = artisan.ApplicationUserId,
                FullName = artisan.User.FullName,
                JobName = artisan.Job?.Name ?? "غير محدد",
                Bio = artisan.Bio,
                ExperienceYears = artisan.ExperienceYears,
                StartingPrice = artisan.StartingPrice,
                ProfilePicture = artisan.User.ProfilePicture,
                CoverPicture = artisan.CoverPicture ?? "/images/covers/default.jpg",
                WorkHours = artisan.WorkHours,
                ServiceArea = artisan.ServiceArea,
                ResponseTime = artisan.ResponseTime,
                IsEmergencyAvailable = artisan.IsEmergencyAvailable,
                PortfolioImages = artisan.Portfolio.Select(p => p.ImageUrl).ToList(),
                Rating = artisan.Reviews.Any() ? (double)artisan.Reviews.Average(r => r.Rating) : 0,
                JoinedDate = artisan.User.CreatedAt,
                CompletedOrdersCount = artisan.Reviews.Count(),
                Governorate = $"{artisan.User.Governorate} - {artisan.ServiceArea}"
            };
        }

        // 2. تحديث بيانات الحرفي
        public async Task<bool> UpdateArtisanProfileAsync(string userId, UpdateArtisanProfileDto dto)
        {
            var artisan = await _context.Artisans
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return false;

            artisan.User.Email = dto.Email;
            artisan.User.PhoneNumber = dto.PhoneNumber;
            artisan.User.Governorate = dto.Governorate;
            artisan.Bio = dto.Bio;
            artisan.ExperienceYears = dto.ExperienceYears;
            artisan.StartingPrice = dto.StartingPrice;
            artisan.ServiceArea = dto.ServiceArea;
            artisan.WorkHours = dto.WorkHours;
            artisan.ResponseTime = dto.ResponseTime;
            artisan.IsEmergencyAvailable = dto.IsEmergencyAvailable;

            if (dto.Services != null && dto.Services.Any())
            {
                artisan.Services = string.Join(", ", dto.Services);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // 3. إضافة صورة لمعرض الأعمال
        public async Task<bool> AddImageToPortfolioAsync(string userId, AddPortfolioImageDto dto)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            string imageUrl = await _fileService.UploadImageAsync(dto.Image, "portfolio");
            if (string.IsNullOrEmpty(imageUrl)) return false;

            var newImage = new ArtisanPortfolio
            {
                ArtisanId = artisan.Id,
                ImageUrl = imageUrl,
                Description = dto.Description
            };

            _context.ArtisanPortfolios.Add(newImage);
            return await _context.SaveChangesAsync() > 0;
        }

        // 4. عرض طلبات الحرفي  
        public async Task<List<ArtisanOrderDto>> GetArtisanOrdersAsync(string artisanId, string? status = null)
        {
            var query = _context.ServiceRequests
                .Include(r => r.Client).ThenInclude(c => c.User)
                .Where(r => r.Artisan.ApplicationUserId == artisanId);

            // الفلترة بناءً على الحالة
            if (!string.IsNullOrEmpty(status) && status != "الكل")
            {
                query = query.Where(r => r.Status.ToString() == status);
            }

            return await query
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new ArtisanOrderDto
                {
                    OrderId = r.Id,
                    ClientName = r.Client.User.FullName,
                    Location = $"{r.Client.User.Governorate} - {r.Client.Address}",
                    OrderDate = r.RequestDate.ToString("yyyy-MM-dd"),
                    ServiceName = r.ServiceName ?? "خدمة عامة",
                    Status = r.Status.ToString()
                })
                .ToListAsync();
        }

        // 5. تحديث حالة الطلب
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var request = await _context.ServiceRequests.FindAsync(orderId);
            if (request == null) return false;

            if (Enum.TryParse<RequestStatus>(newStatus, true, out var statusEnum))
            {
                request.Status = statusEnum;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        // 6. حذف الحرفي
        public async Task<bool> DeleteArtisanAsync(string userId)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            _context.Artisans.Remove(artisan);
            return await _context.SaveChangesAsync() > 0;
        }

        // 7. حذف صورة من المعرض
        public async Task<bool> DeleteImageFromPortfolioAsync(int imageId)
        {
            var image = await _context.ArtisanPortfolios.FindAsync(imageId);
            if (image == null) return false;

            _fileService.DeleteImage(image.ImageUrl);
            _context.ArtisanPortfolios.Remove(image);
            return await _context.SaveChangesAsync() > 0;
        }

        // 8. إضافة أو تحديث تقييم
        public async Task<bool> AddOrUpdateReviewAsync(string clientId, UpdateReviewDto dto)
        {
            var artisanExists = await _context.Artisans.AnyAsync(a => a.Id == dto.ArtisanId);
            if (!artisanExists) return false;

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewerId == clientId && r.ArtisanId == dto.ArtisanId);

            if (existingReview != null)
            {
                existingReview.Rating = (decimal)dto.Rating;
                existingReview.Comment = dto.Comment;
                existingReview.CreatedAt = DateTime.Now;
            }
            else
            {
                var newReview = new Review
                {
                    ReviewerId = clientId,
                    ArtisanId = dto.ArtisanId,
                    Rating = (decimal)dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.Now
                };
                _context.Reviews.Add(newReview);
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
//.