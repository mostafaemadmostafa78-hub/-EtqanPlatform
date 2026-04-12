using ETQAN.API.Data;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context) => _context = context;

        // 1.  صفحة "تواصل معنا" العامة
        [HttpPost("send-general")]
        public async Task<IActionResult> SendMessage([FromBody] ContactFormDto dto)
        {
            var newMessage = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Content = dto.MessageContent,
                ComplaintText = dto.Complaint, // لو ملى خانة الشكوى في الصفحة العامة
                Subject = "رسالة عامة من الموقع",
                SentAt = DateTime.Now
            };

            _context.ContactMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم استلام رسالتك بنجاح، وسنتواصل معك قريباً." });
        }

        // 2.ميثود "تقديم شكوى" ضد حرفي من البروفايل أو التقييمات
        [Authorize] // حماية: لازم يكون عامل Login
        [HttpPost("report-artisan")]
        public async Task<IActionResult> ReportArtisan([FromBody] SubmitComplaintDto dto)
        {
            // سحب بيانات العميل "أوتوماتيك" من الـ Token
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var report = new ContactMessage
            {
                Name = userName ?? "عميل مسجل",
                Email = userEmail ?? "No Email",
                ComplaintText = dto.ComplaintText,
                ArtisanId = dto.ArtisanId,
                Subject = "شكوى ضد حرفي من صفحة التقييمات", // تمييز المصدر للأدمن
                SentAt = DateTime.Now
            };

            _context.ContactMessages.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم إرسال شكوتك بنجاح وسنراجع الأمر." });
        }
    }
}