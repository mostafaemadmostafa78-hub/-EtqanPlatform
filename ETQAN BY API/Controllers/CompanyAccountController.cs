using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyAccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;
        private readonly IEmailServices _emailServices;

        public CompanyAccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMemoryCache cache,
            IEmailServices emailServices)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _cache = cache;
            _emailServices = emailServices;
        }

        // ==========================================
        // الخطوة الأولى: رفع الملف، توليد OTP، وحفظ مؤقت
        // ==========================================
        [HttpPost("register-step1-send-otp")]
        public async Task<IActionResult> RegisterStep1([FromForm] RegisterCompanyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string normalizedEmail = dto.Email.ToLower().Trim();

            // 1. التحقق من وجود الإيميل
            if (await _userManager.FindByEmailAsync(normalizedEmail) != null)
                return BadRequest(new { message = "هذا البريد الإلكتروني مسجل بالفعل" });

            // 2. معالجة وحفظ ملف السجل التجاري
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/companies");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.CommercialRegisterFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.CommercialRegisterFile.CopyToAsync(stream);
            }

            // 3. توليد الكود OTP
            string otp = new Random().Next(100000, 999999).ToString();

            // 4. حفظ البيانات في الكاش (بما فيها مسار الملف)
            var cacheEntry = new CompanyOtpCacheEntry
            {
                CompanyName = dto.CompanyName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Password = dto.Password,
                SavedFilePath = "/uploads/companies/" + fileName,
                OtpCode = otp
            };

            _cache.Set($"company_otp_{normalizedEmail}", cacheEntry, TimeSpan.FromMinutes(60));

            // 5. إرسال الإيميل
            try
            {
                _emailServices.SendEmail(new EmailDTO
                {
                    To = normalizedEmail,
                    Subject = "كود تفعيل حساب شركة - إتقان",
                    Body = $"<div style='direction:rtl;'><h2>كود تفعيل الشركة هو: {otp}</h2><p>هذا الكود صالح لمدة 15 دقيقة.</p></div>"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل إرسال الإيميل، تأكد من الإعدادات", detail = ex.Message });
            }

            return Ok(new { message = "تم إرسال كود التحقق بنجاح" });
        }

        // ==========================================
        // الخطوة الثانية: التحقق وإنشاء الحساب النهائي
        // ==========================================
        [HttpPost("register-step2-verify")]
        public async Task<IActionResult> RegisterStep2([FromBody] VerifyOtpDto request)
        {
            string normalizedEmail = request.Email.ToLower().Trim();
            string cacheKey = $"company_otp_{normalizedEmail}";

            if (!_cache.TryGetValue(cacheKey, out CompanyOtpCacheEntry cachedData))
            {
                return BadRequest(new { message = "انتهت صلاحية الكود أو البريد غير موجود" });
            }

            if (cachedData.OtpCode != request.Otp)
            {
                return BadRequest(new { message = "كود التحقق غير صحيح" });
            }

            // استخدام الـ Transaction لضمان إنشاء الـ User والـ Role والبروفايل معاً
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new ApplicationUser
                {
                    FullName = cachedData.CompanyName,
                    Email = cachedData.Email,
                    UserName = cachedData.Email,
                    PhoneNumber = cachedData.PhoneNumber,
                    CompanyName = cachedData.CompanyName,
                    CommercialRegister = cachedData.SavedFilePath, // المسار اللي حفظناه في Step 1
                    UserType = UserType.Company,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, cachedData.Password);
                if (!result.Succeeded) return BadRequest(result.Errors);

                // التأكد من وجود الـ Role
                if (!await _roleManager.RoleExistsAsync("Company"))
                    await _roleManager.CreateAsync(new IdentityRole("Company"));

                await _userManager.AddToRoleAsync(user, "Company");

                await transaction.CommitAsync();
                _cache.Remove(cacheKey);

                return Ok(new { message = "تم إنشاء حساب الشركة وتفعيله بنجاح!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "حدث خطأ أثناء حفظ البيانات", detail = ex.Message });
            }
        }
    }
}