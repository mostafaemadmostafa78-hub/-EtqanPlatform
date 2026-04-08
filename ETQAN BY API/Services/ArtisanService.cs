//ah
using ETQAN.API.Data;
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

        // 1. عرض تفاصيل الحرفي (لصفحة البروفايل)
        public async Task<ArtisanDetailsDto> GetArtisanDetailsAsync(string userId)
        {
            var artisan = await _context.Artisans
                .Include(a => a.User)
                .Include(a => a.Job)
                .Include(a => a.Portfolio)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return null;

            var completedOrders = await _context.ServiceRequests
        .CountAsync(r => r.Artisan.ApplicationUserId == userId && r.Status == RequestStatus.Finished);

            return new ArtisanDetailsDto
            {
                Id = artisan.ApplicationUserId,
                FullName = artisan.User.FullName,
                JobName = artisan.Job.Name,
                Bio = artisan.Bio,
                ExperienceYears = artisan.ExperienceYears,
                StartingPrice = artisan.StartingPrice,
                ProfilePicture = artisan.User.ProfilePicture,
                Governorate = artisan.User.Governorate,
                WorkHours = artisan.WorkHours,
                ServiceArea = artisan.ServiceArea,
                ResponseTime = artisan.ResponseTime,
                IsEmergencyAvailable = artisan.IsEmergencyAvailable,
                PortfolioImages = artisan.Portfolio.Select(p => p.ImageUrl).ToList(),
                Rating = 4.8 // ثابتة مؤقتاً
            };
        }

        // 2. تحديث بيانات الحرفي (الـ 12 خانة )
        
        public async Task<bool> UpdateArtisanProfileAsync(string userId, UpdateArtisanProfileDto dto)
        {
            var artisan = await _context.Artisans
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return false;

            // 1. تحديث بيانات المستخدم الأساسية
            artisan.User.Email = dto.Email;
            artisan.User.PhoneNumber = dto.PhoneNumber;
            artisan.User.Governorate = dto.Governorate;

            // 2. تحديث بيانات الحرفي المهنية (الـ 12 خانة)
            artisan.Bio = dto.Bio;
            artisan.ExperienceYears = dto.ExperienceYears;
            artisan.StartingPrice = dto.StartingPrice;
            artisan.ServiceArea = dto.ServiceArea;
            artisan.WorkHours = dto.WorkHours;
            artisan.ResponseTime = dto.ResponseTime;
            artisan.IsEmergencyAvailable = dto.IsEmergencyAvailable;

            // 3. تحديث قائمة الخدمات
            if (dto.Services != null && dto.Services.Any())
            {
                artisan.Services = string.Join(", ", dto.Services);
            }

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        

        // 3. إضافة صورة لمعرض الأعمال
        public async Task<bool> AddImageToPortfolioAsync(string userId, AddPortfolioImageDto dto)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            try
            {
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
            catch (Exception ex)
            {
                throw new Exception("Error saving image: " + ex.Message);
            }
        }

        // 4. عرض طلبات الحرفي
        public async Task<IEnumerable<ArtisanOrderDto>> GetMyOrdersAsync(string artisanId)
        {
            return await _context.ServiceRequests
                .Include(r => r.Client).ThenInclude(c => c.User)
                .Where(r => r.Artisan.ApplicationUserId == artisanId)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new ArtisanOrderDto
                {
                    OrderId = r.Id,
                    ClientName = r.Client.User.FullName,
                    Location = r.Client.User.Governorate ?? "غير محدد",
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
    }
}
//.