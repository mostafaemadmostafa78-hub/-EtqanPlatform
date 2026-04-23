using Etqan.Hubs;
using ETQAN.API.Data;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.SignalR;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // ميثود لإرسال الإشعار وحفظه مع تحديد نوعه
    public async Task SendNotificationAsync(string userId, string title, string message, string type, string? actionUrl = null)
    {
        // 1. تخزين التنبيه في قاعدة البيانات مع تحديد النوع
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = type, // تحديد هل هو شات أم طلب
            ActionUrl = actionUrl,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // 2. إرسال التنبيه للمستخدم بشكل لحظي عبر الـ Hub
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
        {
            title = notification.Title,
            message = notification.Message,
            type = notification.NotificationType,
            actionUrl = notification.ActionUrl,
            createdAt = notification.CreatedAt
        });
    }
}