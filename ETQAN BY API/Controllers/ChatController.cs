using ETQAN.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    /// <summary>
    /// المتحكم المسؤول عن إدارة عمليات الدردشة الملحقة وتداول الملفات والوسائط
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChatController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        #region إدارة رفع الوسائط (صور - صوت)

        [HttpPost("upload-chat-file")]
        public async Task<IActionResult> UploadChatFile(IFormFile file, [FromQuery] string fileType)
        {
            if (file == null || file.Length == 0) return BadRequest("لم يتم اختيار ملف للرفع.");

            // تحديد مسار المجلد بناءً على نوع الملف (chats/images أو chats/audio)
            string subFolder = fileType == "audio" ? "audio" : "images";
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "chats", subFolder);

            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/chats/{subFolder}/{fileName}";
            return Ok(new { url = fileUrl });
        }
        #endregion

        #region جلب البيانات والمحادثات

        /// <summary>
        /// جلب قائمة المحادثات النشطة مع إمكانية البحث في أسماء جهات الاتصال
        /// </summary>
        [HttpGet("my-conversations")]
        public async Task<IActionResult> GetMyConversations([FromQuery] string? search = null)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // استعلام لجلب المحادثات وتجميعها حسب الطرف الآخر
            var query = _context.ChatMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId);

            var conversations = await query
                .Select(g => new {
                    ContactId = g.Key,
                    // جلب بيانات الطرف الآخر من جدول المستخدمين
                    ContactName = _context.Users.Where(u => u.Id == g.Key).Select(u => u.FullName).FirstOrDefault(),
                    ContactImage = _context.Users.Where(u => u.Id == g.Key).Select(u => u.ProfilePicture).FirstOrDefault(),
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault().MessageContent,
                    LastMessageDate = g.Max(m => m.Timestamp)
                })
                // تطبيق فلترة البحث في حالة وجود كلمة مفتاحية
                .Where(c => string.IsNullOrEmpty(search) || c.ContactName.Contains(search))
                .OrderByDescending(c => c.LastMessageDate)
                .ToListAsync();

            return Ok(conversations);
        }

        /// <summary>
        /// استرجاع السجل الكامل للمحادثة بين المستخدم الحالي ومستخدم آخر
        /// </summary>
        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(string otherUserId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new {
                    m.SenderId,
                    m.ReceiverId,
                    m.MessageContent,
                    m.ImageUrl,
                    m.IsImage,
                    m.AudioUrl,
                    m.IsAudio,
                    m.Timestamp
                })
                .ToListAsync();

            return Ok(messages);
        }
        #endregion
    }
}