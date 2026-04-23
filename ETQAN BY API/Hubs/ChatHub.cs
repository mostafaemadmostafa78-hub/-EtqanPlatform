using ETQAN.API.Data;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;

namespace ETQAN_BY_API.Hubs
{
    /// <summary>
    /// المسؤول عن إدارة المحادثات الفورية والتحقق من سياسات الخصوصية Hub كلاس الـ
    /// </summary>
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public ChatHub(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        #region الرسائل النصية
        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;

            // التحقق من سياسات الخصوصية ومنع تبادل أرقام الهاتف أو البريد الإلكتروني
            if (ContainsSensitiveData(message))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "يمنع تبادل بيانات التواصل الشخصية لضمان سلامة التعاملات.");
                return;
            }

            // جلب بيانات مرسل الرسالة لتضمين الاسم في التنبيهات
            var sender = await _context.Users.FindAsync(senderId);
            var senderName = sender?.FullName ?? "مستخدم";

            // أرشفة الرسالة في قاعدة البيانات
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

            // بث الرسالة للمستلم بشكل لحظي
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, null);

            // إرسال إشعار دفع وتصنيفه كرسالة محادثة
            await _notificationService.SendNotificationAsync(
                receiverId,
                "رسالة جديدة من " + senderName,
                message.Length > 25 ? message.Substring(0, 25) + "..." : message,
                "Chat", // نوع الإشعار للفلترة في الفرونت إند
                "/chat/" + senderId
            );
        }
        #endregion

        #region الوسائط المتعددة (صور - صوت)
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
                Timestamp = DateTime.Now
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

        public async Task SendAudio(string receiverId, string audioUrl)
        {
            var senderId = Context.UserIdentifier;
            var sender = await _context.Users.FindAsync(senderId);
            var senderName = sender?.FullName ?? "مستخدم";

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
                "أرسل لك تسجيلاً صوتياً",
                "Chat",
                "/chat/" + senderId
            );
        }
        #endregion

        #region ميثودز التحقق المساعدة
        /// <summary>
        ///Regex فحص النص والتأكد من خلوه من أرقام الهاتف أو الإيميلات باستخدام 
        /// </summary>
        private bool ContainsSensitiveData(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            // نمط للبحث عن الإيميلات وأرقام الهواتف (بين 10 إلى 14 رقم)
            string pattern = @"(\+?\d{10,14}|[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})";
            return Regex.IsMatch(text, pattern);
        }
        #endregion
    }
}