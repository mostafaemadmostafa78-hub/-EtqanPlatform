//ah
using ETQAN.API.Data;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtisanReviewsController : ControllerBase
    {
        private readonly IArtisanService _artisanService;
        private readonly ApplicationDbContext _context;

        public ArtisanReviewsController(ApplicationDbContext context, IArtisanService artisanService)
        {
            _context = context;
            _artisanService = artisanService;
        }

        /// <summary>
        /// إضافة تقييم جديد أو تحديث تقييم موجود مسبقاً
        /// </summary>
        [HttpPost("submit")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> SubmitReview([FromBody] ArtisanReviewCreateDto dto)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(clientId))
                return Unauthorized(new { message = "يجب تسجيل الدخول كعميل لتتمكن من التقييم" });

            var result = await _artisanService.AddOrUpdateArtisanReviewAsync(clientId, dto);

            if (result)
            {
                return Ok(new { message = "تم حفظ تقييمك بنجاح، شكراً لمشاركتك!" });
            }

            return BadRequest(new { message = "فشل حفظ التقييم. تأكد من صحة رقم الطلب وبيانات التقييم." });
        }
        [HttpGet("my-reviews")]
        [Authorize(Roles = "Artisan")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return NotFound("الحرفي غير موجود");

            var reviews = await _context.Reviews
                .Where(r => r.Artisan != null && r.Artisan.ApplicationUserId == userId)
                .Include(r => r.Reviewer) 
                .OrderByDescending(r => r.CreatedAt)   
                .Select(r => new {
                    CustomerName = r.Reviewer.FullName,
                    Rating = r.Rating.ToString("0.0"),
                    Comment = r.Comment,
                    Date = r.CreatedAt.ToString("yyyy/MM/dd")
                })
                .ToListAsync();

            return Ok(reviews);
        }

    }
}
//.