//ah
using ETQAN.API.Data;
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
        public async Task<IActionResult> SubmitReview([FromBody] UpdateReviewDto dto)
        {
            // 1. جلب الـ ID بتاع العميل من الـ Token 
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(clientId))
                return Unauthorized(new { message = "يجب تسجيل الدخول كعميل لتتمكن من التقييم" });

            // 2. إرسال الـ OrderId والتقييم للـ Service للتحقق والحفظ
            var result = await _artisanService.AddOrUpdateReviewAsync(clientId, dto);

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
            // 1. نجيب الـ ID بتاع الحرفي من التوكن (أمان)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artisan = await _context.Artisans.FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            if (artisan == null) return NotFound("الحرفي غير موجود");

            // 2. نجيب كل التقييمات اللي اتعملت له
            var reviews = await _context.Reviews
                .Where(r => r.ArtisanId == artisan.Id)
                .Include(r => r.Reviewer) // عشان يشوف اسم العميل اللي قيمه
                .OrderByDescending(r => r.CreatedAt) // الأحدث يظهر الأول
                .Select(r => new {
                    CustomerName = r.Reviewer.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Date = r.CreatedAt.ToString("yyyy/MM/dd")
                })
                .ToListAsync();

            return Ok(reviews);
        }

    }
}
//.