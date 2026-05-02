using ETQAN.API.Data;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETQAN_BY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context) => _context = context;

        [HttpPost("send-general")]
        public async Task<IActionResult> SendMessage([FromBody] ContactFormDto dto)
        {
            if (string.IsNullOrEmpty(dto.MessageContent) && string.IsNullOrEmpty(dto.Complaint))
            {
                return BadRequest("يجب إدخال رسالة أو شكوى.");
            }

            var newMessage = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Content = dto.MessageContent,
                ComplaintText = dto.Complaint,
                ArtisanId = dto.ArtisanId,
                CompanyId = dto.CompanyId,
                Subject = !string.IsNullOrEmpty(dto.Complaint)
                    ? (dto.ArtisanId != null ? "شكوى ضد حرفي" : (dto.CompanyId != null ? "شكوى ضد شركة" : "شكوى عامة"))
                    : "استفسار عام",
                SentAt = DateTime.Now
            };

            _context.ContactMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم استلام رسالتك بنجاح." });
        }

        [HttpGet("admin/messages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
            return Ok(messages);
        }
    }
}