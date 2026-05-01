using ETQAN.API.Data;
using ETQAN.API.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
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

        [Authorize]
        [HttpPost("mark-as-read/{senderId}")]
        public async Task<IActionResult> MarkAsRead(string senderId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var messages = await _context.ChatMessages
                .Where(m => m.SenderId == senderId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();

            messages.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("my-conversations")]
        public async Task<IActionResult> GetMyConversations([FromQuery] string? search = null)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var query = _context.ChatMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId);

            var conversations = await query
                .Select(g => new {
                    ContactId = g.Key,
                    ContactName = _context.Users
                        .Where(u => u.Id == g.Key)
                        .Select(u => u.UserType == UserType.Company ? u.CompanyName : u.FullName)
                        .FirstOrDefault(),
                    ContactImage = _context.Users.Where(u => u.Id == g.Key).Select(u => u.ProfilePicture).FirstOrDefault(),
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault().MessageContent,
                    LastMessageDate = g.Max(m => m.Timestamp),
                    UnreadCount = g.Count(m => m.ReceiverId == currentUserId && !m.IsRead)
                })
                .Where(c => string.IsNullOrEmpty(search) ||
                            c.ContactName.Contains(search) ||
                            c.LastMessage.Contains(search))
                .OrderByDescending(c => c.LastMessageDate)
                .ToListAsync();

            return Ok(conversations);
        }

        [HttpPost("upload-chat-image")]
        public async Task<IActionResult> UploadChatImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("لم يتم اختيار صورة للرفع.");

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "chats", "images");

            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/chats/images/{fileName}";
            return Ok(new { url = fileUrl });
        }

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
                    m.Timestamp,
                    m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}