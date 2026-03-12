using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // مهم جداً للعمليات غير المتزامنة

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtisanAccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ArtisanAccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterArtisanDto dto)
        {
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // 2. التحقق من تكرار البريد الإلكتروني
                var userExists = await _userManager.FindByEmailAsync(dto.Email);
                if (userExists != null)
                    return BadRequest(new { message = "هذا البريد الإلكتروني مسجل بالفعل" });

                // 3. التحقق من تكرار الرقم القومي (بشكل Async)
                var nationalIdExists = await _context.Artisans.AnyAsync(a => a.NationalId == dto.NationalId);
                if (nationalIdExists)
                    return BadRequest(new { message = "هذا الرقم القومي مسجل مسبقاً لمستخدم آخر" });

                // 4. التحقق من وجود المهنة في قاعدة البيانات
                var jobExists = await _context.Jobs.AnyAsync(j => j.Id == dto.JobId);
                if (!jobExists)
                    return BadRequest(new { message = "المهنة المختارة غير صالحة أو غير موجودة" });

                // 5. إنشاء كائن المستخدم (Identity User)
                var user = new ApplicationUser
                {
                    FullName = dto.Fullname,
                    Email = dto.Email,
                    UserName = dto.Email, // نستخدم الإيميل كـ UserName غالباً في الـ APIs
                    UserType = UserType.Artisan,
                    EmailConfirmed = false // سيتم تفعيلها بعد الـ OTP
                };

                // 6. حفظ المستخدم في Identity (تشفير كلمة السر يتم هنا تلقائياً)
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new { errors });
                }

                // 7. إضافة المستخدم لدور "Artisan" (تأكد من وجود الدور في قاعدة البيانات)
                if (!await _roleManager.RoleExistsAsync("Artisan"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Artisan"));
                }
                await _userManager.AddToRoleAsync(user, "Artisan");

                // 8. إنشاء سجل الحرفي وربطه بالمستخدم
                var artisan = new Artisan
                {
                    Age = dto.Age,
                    NationalId = dto.NationalId,
                    // تحويل الرقم القادم من React إلى Enum إذا كان معرفاً كذلك في الـ Model
                    MaritalStatus = (MaritalStatus)dto.MaritalStatus,
                    JobId = dto.JobId,
                    ApplicationUserId = user.Id,
                  //  CreatedAt = DateTime.Now
                };

                _context.Artisans.Add(artisan);
                await _context.SaveChangesAsync();

                // 9. النجاح (نرسل userId لاستخدامه في صفحة الـ OTP إذا لزم الأمر)
                return Ok(new
                {
                    message = "تم إنشاء الحساب بنجاح، يرجى تفعيل البريد الإلكتروني",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ (Logging)
                return StatusCode(500, new { message = "حدث خطأ داخلي في الخادم", details = ex.Message });
            }
        }
    }
}