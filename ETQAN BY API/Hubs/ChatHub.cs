using ETQAN.API.Data;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System; 

namespace ETQAN_BY_API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService; // 1. تعريف السيرفر هنا

        // 2. تحديث الكونستركتور عشان يستقبل السيرفر
        public ChatHub(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // ميثود إرسال وحفظ الرسائل النصية
        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;

            // 1. هنجيب بيانات الراسل من جدول المستخدمين عشان ناخد الـ FullName
            var sender = await _context.Users.FindAsync(senderId);
            var senderName = sender?.FullName ?? "مستخدم";

            // 2. حفظ الرسالة في الداتابيز
            var chatMsg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageContent = message,
                IsImage = false,
                Timestamp = DateTime.Now
            };
            _context.ChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            // 3. إرسال الرسالة للشات
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, null);

            // 4. إرسال الإشعار بالاسم الحقيقي
            await _notificationService.SendNotificationAsync(
                receiverId,
                "رسالة جديدة من " + senderName,
                message.Length > 20 ? message.Substring(0, 20) + "..." : message, // بنعرض أول جزء من الرسالة في الإشعار
                "/chat/" + senderId
            );
        }

        // ميثود إرسال وحفظ الصور
        public async Task SendImage(string receiverId, string imageUrl)
        {
            var senderId = Context.UserIdentifier;
            var senderName = Context.User.Identity.Name ?? "مستخدم";

            // 1. حفظ في الداتابيز
            var chatMsg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                ImageUrl = imageUrl,
                IsImage = true,
                Timestamp = DateTime.Now
            };
            _context.ChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            // 2. إرسال الرابط لحظياً للمستلم
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, null, imageUrl);

            // 3. إرسال إشعار للمستلم إن فيه صورة وصلت
            await _notificationService.SendNotificationAsync(
                receiverId,
                "صورة جديدة من " + senderName,
                "أرسل لك صورة في المحادثة",
                "/chat/" + senderId
            );
        }

        // ميثود إرسال الريكورد
        public async Task SendAudio(string receiverId, string audioUrl)
        {
            var senderId = Context.UserIdentifier;
            var senderName = Context.User.Identity.Name ?? "مستخدم";

            var chatMsg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                AudioUrl = audioUrl,
                IsAudio = true,
                Timestamp = DateTime.Now
            };
            _context.ChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, null, null, audioUrl);

            await _notificationService.SendNotificationAsync(
                receiverId,
                "رسالة صوتية من " + senderName,
                "أرسل لك ريكورد في المحادثة",
                "/chat/" + senderId
            );
        }
    }
}