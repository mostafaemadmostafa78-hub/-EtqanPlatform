using ETQAN.API.Data;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public ServiceRequestController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost("respond-to-request/{id}")]
        public async Task<IActionResult> RespondToRequest(int id, [FromBody] bool accept)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null) return NotFound("الطلب غير موجود");

            serviceRequest.Status = accept ? RequestStatus.Accepted : RequestStatus.Cancelled;
            await _context.SaveChangesAsync();

            var artisan = await _context.Users.FindAsync(serviceRequest.ArtisanId);
            var artisanName = artisan?.FullName ?? "الحرفي";

            string notificationTitle = accept ? "تحديث حالة الطلب" : "إشعار بشأن طلبك";
            string statusMessage = accept ? "تمت الموافقة على" : "نعتذر، تم رفض";

            await _notificationService.SendNotificationAsync(
                serviceRequest.ClientId.ToString(),
                notificationTitle,
                $"قام السيد/ {artisanName} بـ {statusMessage} طلبكم لخدمة ({serviceRequest.ServiceName})",
                "/client/requests"
            );

            return Ok(new
            {
                success = true,
                message = accept ? "تم قبول الطلب وتنبيه العميل" : "تم رفض الطلب وإبلاغ العميل"
            });
        }
    }
}