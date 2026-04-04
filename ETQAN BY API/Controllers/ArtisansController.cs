using ETQAN.API.Data;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class ArtisansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //ah
        private readonly IArtisanService _artisanService; 

        
        public ArtisansController(ApplicationDbContext context, IArtisanService artisanService)
        {
            _context = context;
            _artisanService = artisanService; // هنا بنربط الخدمة
        }
        //.
 

        
        [HttpGet]
       // [Authorize(Roles = "Client,Admin")]

        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Artisans.Include(a => a.User).Include(a => a.Job).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.User.FullName.Contains(search) || a.Job.Name.Contains(search));
            }

            var result = await query.Select(a => new ArtisanListDto
            {
                Id = a.ApplicationUserId,
                Name = a.User.FullName,
                JobName = a.Job.Name,
                Price = a.StartingPrice,
                Rating = 4.8, // قيمة مؤقتة
                ImageUrl = a.User.ProfilePicture ?? "/images/Artisans/default.svg"
            }).ToListAsync();

            return Ok(result);

        }
        [HttpGet("{id}")]
        [Authorize] // مسموح لأي يوزر مسجل (عميل، شركة، إلخ) يشوف البروفايل
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                // 1. جلب الحرفي بكل بياناته المرتبطة (اليوزر، الوظيفة، ومعرض الصور)
                var artisan = await _context.Artisans
                    .Include(a => a.User)
                    .Include(a => a.Job)
                    .Include(a => a.Portfolio)
                    .FirstOrDefaultAsync(a => a.ApplicationUserId == id);

                // 2. التحقق من وجود الحرفي
                if (artisan == null)
                {
                    return NotFound(new { message = "هذا الحرفي غير موجود" });
                }

                // 3. تحويل البيانات إلى DTO (Mapping)
                var result = new ArtisanDetailsDto
                {
                    Id = artisan.ApplicationUserId,
                    FullName = artisan.User.FullName,
                    JobName = artisan.Job?.Name ?? "حرفي",
                    Bio = artisan.Bio ?? "لا يوجد نبذة تعريفية حالياً",
                    ExperienceYears = artisan.ExperienceYears,
                    StartingPrice = artisan.StartingPrice,
                    Governorate = artisan.User.Governorate ?? "غير محددة",
                    Rating = 4.8, // قيمة افتراضية حالياً حتى نبرمج جدول التقييمات
                    ProfilePicture = artisan.User.ProfilePicture ?? "/images/Artisans/default.svg",

                    // تحويل قائمة الصور من جدول Portfolio لمجرد قائمة نصوص (URLs)
                    PortfolioImages = artisan.Portfolio != null
                        ? artisan.Portfolio.Select(p => p.ImageUrl).ToList()
                        : new List<string>()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ (Logger) للرجوع إليه
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب بيانات البروفايل", error = ex.Message });
            }
        }
        //ah

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateArtisan(string id, [FromBody] ArtisanDetailsDto dto)
        {
            var result = await _artisanService.UpdateArtisanAsync(id, dto);
            if (!result) return NotFound("الحرفي غير موجود");

            return Ok(new { message = "تم تحديث البيانات بنجاح" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtisan(string id)
        {
            var result = await _artisanService.DeleteArtisanAsync(id);
            if (!result) return NotFound("فشل الحذف، الحرفي غير موجود");

            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }

        // [Authorize(Roles = "Artisan")] // فكي التعليق ده لو عايزة الحرفيين بس اللي يرفعوا
        [HttpPost("portfolio/add")]
        public async Task<IActionResult> AddPortfolioImage([FromForm] AddPortfolioImageDto dto)
        {
            // سحب الـ ID بتاع الحرفي من الـ Token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (dto.Image == null) return BadRequest("يرجى اختيار صورة أولاً");

            var result = await _artisanService.AddImageToPortfolioAsync(userId, dto);

            if (result) return Ok(new { message = "تم إضافة الصورة لمعرض أعمالك بنجاح" });

            return BadRequest("حدث خطأ أثناء رفع الصورة");
        }

        // [Authorize(Roles = "Artisan")]
        [HttpDelete("portfolio/delete/{imageId}")]
        public async Task<IActionResult> DeletePortfolioImage(int imageId)
        {
            var result = await _artisanService.DeleteImageFromPortfolioAsync(imageId);

            if (result) return Ok(new { message = "تم حذف الصورة بنجاح" });

            return NotFound("الصورة غير موجودة أو تم حذفها بالفعل");
        }
        //.
    }


} 
