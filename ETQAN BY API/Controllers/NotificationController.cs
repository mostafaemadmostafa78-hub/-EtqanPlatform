using ETQAN.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Claims;
using MailKit.Net.Smtp;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications([FromQuery] string? type = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _context.Notifications.Where(n => n.UserId == userId);

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(n => n.NotificationType == type);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new {
                n.Id,
                n.Title,
                n.Message,
                n.NotificationType,
                n.ActionUrl,
                n.IsRead,
                TimeAgo = GetTimeAgo(n.CreatedAt)
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return Ok(new { unreadCount = count });
    }

    [HttpPost("mark-as-read")]
    public async Task<IActionResult> MarkAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var item in unread) { item.IsRead = true; }
        await _context.SaveChangesAsync();

        return Ok(new { message = "تم تحديث الحالة لجميع التنبيهات" });
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.Now - dateTime;
        if (span.TotalDays > 1) return $"منذ {Math.Floor(span.TotalDays)} يوم";
        if (span.TotalHours > 1) return $"منذ {Math.Floor(span.TotalHours)} ساعة";
        return $"منذ {Math.Floor(span.TotalMinutes)} دقيقة";
    }

}