using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientAccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ClientAccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // =========================
        // Register Client
        // =========================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterClientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest("Email already exists");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Governorate = dto.Governorate,
                UserType = UserType.Client
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Client");

            // إضافة Client في جدول Clients
            var client = new Client
            {
                Address = dto.Governorate,
                ApplicationUserId = user.Id
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return Ok("Client registered successfully");
        }


        [Authorize(Roles = "Client")]
        [HttpGet("full-profile")]
        public async Task<IActionResult> GetFullProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { message = "غير مصرح" });

            // ✅ جلب المستخدم مع الـ Client والـ ServiceRequests
            var user = await _userManager.Users
                .Include(u => u.Client)
                    .ThenInclude(c => c.Requests)
                        .ThenInclude(r => r.Artisan)
                            .ThenInclude(a => a.Job)
                .Include(u => u.Client)
                    .ThenInclude(c => c.Requests)
                        .ThenInclude(r => r.Artisan)
                            .ThenInclude(a => a.User)
                .Include(u => u.ReviewsWritten)
                    .ThenInclude(r => r.Artisan)
                        .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود" });

            // ✅ حساب متوسط التقييم
            var overallRating = user.ReviewsWritten != null && user.ReviewsWritten.Any()
                ? (double)user.ReviewsWritten.Average(r => r.Rating)
                : 0.0;

            var profileData = new ClientProfileFullDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Governorate = user.Governorate,

                // ✅ السجلات من Client.Requests
                History = user.Client?.Requests?.Select(r => new HistoryItemDto
                {
                    RequestId = r.Id,
                    ProviderName = r.Artisan?.User?.FullName ?? "غير محدد",
                    JobTitle = r.Artisan?.Job?.Name ?? "غير محدد",
                    Status = r.Status.ToString(),
                    Date = r.RequestDate
                }).ToList() ?? new List<HistoryItemDto>(),

                // ✅ التقييمات من ReviewsWritten
                Reviews = user.ReviewsWritten?.Select(r => new ReviewDto
                {
                    ReviewerName = user.FullName,
                    ReviewerJob = r.Artisan?.Job?.Name ?? "غير محدد",
                    Comment = r.Comment ?? "",
                    Rating = r.Rating,
                    Date = DateTime.Now
                }).ToList() ?? new List<ReviewDto>(),

                OverallRating = Math.Round(overallRating, 1)
            };

            return Ok(profileData);
        }

        // =========================
        // Update Profile
        // =========================
        [Authorize(Roles = "Client")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { message = "غير مصرح" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "المستخدم غير موجود" });

            // ✅ لو الإيميل اتغير نتحقق إنه مش مكرر
            if (user.Email != dto.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(dto.Email);
                if (emailExists != null)
                    return BadRequest(new { message = "البريد الإلكتروني مستخدم مسبقاً" });

                user.Email = dto.Email;
                user.UserName = dto.Email;
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Governorate = dto.Governorate;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            // ✅ تحديث جدول Client
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (client != null)
            {
                client.Address = dto.Governorate;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "تم التحديث بنجاح" });
        }
    }
}