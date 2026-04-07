//ah
using ETQAN.API.Data;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ETQAN_BY_API.Services
{
    public class ArtisanService : IArtisanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService; // لازم نضيف دي هنا

        // لازم نعدل الـ Constructor عشان يستقبل الـ FileService
        public ArtisanService(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<bool> UpdateArtisanAsync(string userId, ArtisanDetailsDto dto)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            artisan.Bio = dto.Bio;
            artisan.ExperienceYears = dto.ExperienceYears;
            artisan.StartingPrice = dto.StartingPrice;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteArtisanAsync(string userId)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            _context.Artisans.Remove(artisan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddImageToPortfolioAsync(string userId, AddPortfolioImageDto dto)
        {
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
            if (artisan == null) return false;

            // رفع الصورة
            string imageUrl = await _fileService.UploadImageAsync(dto.Image, "portfolio");

            var newImage = new ArtisanPortfolio
            {
                ArtisanId = artisan.Id,
                ImageUrl = imageUrl,
                Description = dto.Description
            };

            _context.ArtisanPortfolios.Add(newImage);
            await _context.SaveChangesAsync();
            return true;
        }

       
        public async Task<bool> DeleteImageFromPortfolioAsync(int imageId)
        {
            var image = await _context.ArtisanPortfolios.FindAsync(imageId);
            if (image == null) return false;

            // حذف الملف من الهارد ديسك أولاً
            _fileService.DeleteImage(image.ImageUrl);

            // حذف السجل من الداتابيز
            _context.ArtisanPortfolios.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
//.