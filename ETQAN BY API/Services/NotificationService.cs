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

    public async Task SendNotificationAsync(string userId, string title, string message, string? actionUrl = null)
    {
        // 1. حفظ الإشعار في الداتابيز عشان يفضل موجود في "الجرس"
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // 2. إرسال الإشعار "لحظياً" للمستخدم لو كان فاتح الموقع
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
        {
            title = notification.Title,
            message = notification.Message,
            actionUrl = notification.ActionUrl,
            createdAt = notification.CreatedAt
        });
    }
}