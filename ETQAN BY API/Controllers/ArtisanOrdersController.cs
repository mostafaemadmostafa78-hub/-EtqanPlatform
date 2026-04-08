//ah
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ETQAN_BY_API.Services;
using ETQAN_BY_API.Model.DTOs;

namespace ETQAN_BY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Artisan")] 
    public class ArtisanOrdersController : ControllerBase
    {
        private readonly IArtisanService _artisanService;

        public ArtisanOrdersController(IArtisanService artisanService)
        {
            _artisanService = artisanService;
        }

        // 1. جلب كل الطلبات مع إمكانية الفلترة  
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] string? status = null)
        {
            // بنجيب الـ ID بتاع الحرفي من الـ Token اللي عامل بيه Login
            var artisanId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(artisanId)) return Unauthorized();

            var orders = await _artisanService.GetArtisanOrdersAsync(artisanId, status);
            return Ok(orders);
        }

        // 2. تحديث حالة الطلب (عشان أزرار قبول/رفض/تم الانتهاء)
        [HttpPut("update-status/{orderId}")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromQuery] string newStatus)
        {
            var result = await _artisanService.UpdateOrderStatusAsync(orderId, newStatus);

            if (!result) return BadRequest("تعذر تحديث حالة الطلب. تأكد من الـ OrderId أو الـ Status الصحيح.");

            return Ok(new { message = "تم تحديث حالة الطلب بنجاح" });
        }
    }
}
//.