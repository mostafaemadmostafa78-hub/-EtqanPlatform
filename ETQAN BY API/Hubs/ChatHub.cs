using ETQAN.API.Data;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
namespace ETQAN_BY_API.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notificationService;

    public ChatHub(ApplicationDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task SendMessage(string receiverId, string message)
    {
        var senderId = Context.UserIdentifier;

        if (ContainsSensitiveData(message))
        {
            await Clients.Caller.SendAsync("ErrorMessage", "يمنع تبادل بيانات التواصل الشخصية لضمان سلامة التعاملات.");
            return;
        }

        var sender = await _context.Users.FindAsync(senderId);
        var senderName = sender?.FullName ?? "مستخدم";

        var chatMsg = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            MessageContent = message,
            IsImage = false,
            Timestamp = DateTime.Now,
            IsRead = false
        };

        _context.ChatMessages.Add(chatMsg);
        await _context.SaveChangesAsync();

        await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, null);

        await _notificationService.SendNotificationAsync(
            receiverId,
            "رسالة جديدة من " + senderName,
            message.Length > 25 ? message.Substring(0, 25) + "..." : message,
            "Chat",
            "/chat/" + senderId
        );
    }

    public async Task SendImage(string receiverId, string imageUrl)
    {
        var senderId = Context.UserIdentifier;
        var sender = await _context.Users.FindAsync(senderId);
        var senderName = sender?.FullName ?? "مستخدم";

        var chatMsg = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            ImageUrl = imageUrl,
            IsImage = true,
            Timestamp = DateTime.Now,
            IsRead = false
        };

        _context.ChatMessages.Add(chatMsg);
        await _context.SaveChangesAsync();

        await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, null, imageUrl);

        await _notificationService.SendNotificationAsync(
            receiverId,
            "صورة جديدة من " + senderName,
            "أرسل لك صورة توضيحية",
            "Chat",
            "/chat/" + senderId
        );
    }

    private bool ContainsSensitiveData(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        string pattern = @"(\+?\d{10,14}|[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})";
        return Regex.IsMatch(text, pattern);
    }
}