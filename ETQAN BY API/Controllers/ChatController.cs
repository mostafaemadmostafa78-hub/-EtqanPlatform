using ETQAN.API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ضروري عشان ToListAsync تشتغل
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ETQAN_BY_API.Controllers
{
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

        [HttpPost("upload-chat-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file selected.");

            // التأكد من وجود المجلد
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "chats");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var path = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/chats/{fileName}";
            return Ok(new { url = imageUrl });
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(string otherUserId)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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

        [HttpGet("my-conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var conversations = await _context.ChatMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => new {
                    ContactId = g.Key,
                    // نجلب اسم الطرف الآخر Join مع جدول المستخدمين هنا لعرض الاسم
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault().MessageContent,
                    LastMessageDate = g.Max(m => m.Timestamp)
                })
                .OrderByDescending(c => c.LastMessageDate)
                .ToListAsync();

            return Ok(conversations);
        }

        [HttpPost("upload-chat-audio")]
        public async Task<IActionResult> UploadAudio(IFormFile file) 
        {
            if (file == null || file.Length == 0)
                return BadRequest("No audio file uploaded.");

            // إنشاء مسار الفولدر ( audio المرة دي)
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/chats/audio");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // اسم الملف: Guid عشان نضمن إنه ميتكررش
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // نرجع الرابط 
            var audioUrl = $"/uploads/chats/audio/{fileName}";
            return Ok(new { url = audioUrl });
        }
    }
}