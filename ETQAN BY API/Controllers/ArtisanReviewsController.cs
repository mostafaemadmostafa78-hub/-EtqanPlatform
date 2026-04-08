//ah
using ETQAN_BY_API.Model.DTOs;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtisanReviewsController : ControllerBase
    {
        private readonly IArtisanService _artisanService;

        public ArtisanReviewsController(IArtisanService artisanService)
        {
            _artisanService = artisanService;
        }

        /// <summary>
        /// إضافة تقييم جديد أو تحديث تقييم موجود مسبقاً
        /// </summary>
        [HttpPost("submit")]
        [Authorize(Roles = "Client")] // مسموح فقط للعملاء بالتقييم
        public async Task<IActionResult> SubmitReview([FromBody] UpdateReviewDto dto)
        {
            // 1. جلب معرف العميل (الذي يقوم بالتقييم) من الـ Token
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(clientId))
                return Unauthorized(new { message = "يجب تسجيل الدخول كعميل لتتمكن من التقييم" });

            // 2. استدعاء الخدمة الذكية التي تتعامل مع (Add/Update)
            var result = await _artisanService.AddOrUpdateReviewAsync(clientId, dto);

            if (result)
            {
                return Ok(new { message = "تم حفظ تقييمك بنجاح، شكراً لمشاركتك!" });
            }

            return BadRequest(new { message = "فشل حفظ التقييم. تأكد من صحة بيانات الحرفي والتقييم." });
        }

    }
}
//.