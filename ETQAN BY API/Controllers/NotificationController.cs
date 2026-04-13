using ETQAN.API.Data; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1.  لستة الإشعارات للمستخدم اللي مسجل دخول
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.ActionUrl,
                    n.IsRead,
                    TimeAgo = GetTimeAgo(n.CreatedAt) // ميثود عشان يقول "منذ ....."
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // 2. ميثود عشان عدد التنبيهات الجديدة 
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new { unreadCount = count });
        }

        // 3. تحويل الإشعارات لمقروءة
        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var item in unread) { item.IsRead = true; }
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تحديث جميع التنبيهات" });
        }

        // ميثود لشكل الوقت 
        private static string GetTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalDays > 1) return $"منذ {Math.Floor(span.TotalDays)} يوم";
            if (span.TotalHours > 1) return $"منذ {Math.Floor(span.TotalHours)} ساعة";
            return $"منذ {Math.Floor(span.TotalMinutes)} دقيقة";
        }
    }
}
