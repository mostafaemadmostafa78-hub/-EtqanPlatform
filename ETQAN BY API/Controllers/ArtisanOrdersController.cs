//ah
using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Artisan")]
    public class ArtisanOrdersController : ControllerBase
    {
        private readonly IArtisanService _artisanService;
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public ArtisanOrdersController(IArtisanService artisanService , ApplicationDbContext context, NotificationService notificationService)
        {
            _artisanService = artisanService;
            _context = context;
            _notificationService = notificationService;
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

        // 2. تحديث حالة الطلب عشان أزرار قبول/رفض/تم الانتهاء
        [HttpPut("update-status/{orderId}")]
        public async Task<IActionResult> UpdateStatus(int orderId, [FromQuery] string newStatus)
        {
            var result = await _artisanService.UpdateOrderStatusAsync(orderId, newStatus);

            if (!result) return BadRequest("تعذر تحديث حالة الطلب. تأكد من الـ OrderId أو الـ Status الصحيح.");

            return Ok(new { message = "تم تحديث حالة الطلب بنجاح" });
        }

        // ميثود إصدار الفاتورة النهائية من جهة الحرفي
        [HttpPost("issue-invoice")]
        public async Task<IActionResult> IssueInvoice([FromBody] IssueInvoiceDto dto)
        {
            if (dto.Items.Any(item => item.Price <= 0))
            {
                return BadRequest("يجب ان يكون السعر اكبر من صفر");
            }

            if (dto.Items.Any(item => item.Quantity <= 0))
            {
                return BadRequest("يجب ان تكون الكمية علي الاقل 1");
            }

            var result = await _artisanService.CreateInvoiceFromRequestAsync(dto);
            if (!result) return BadRequest("تعذر إصدار الفاتورة. تأكد من حالة الطلب (Accepted) وصحة البيانات.");

            return Ok(new { message = "تم إصدار الفاتورة بنجاح وتحديث حالة الطلب." });
        }
    }
}
    

//.