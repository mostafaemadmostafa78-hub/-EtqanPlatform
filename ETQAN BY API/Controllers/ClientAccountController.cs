using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Services; // عشان يستخدم VerifyOtpDto
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientAccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;
        private readonly IEmailServices _emailServices;

        public ClientAccountController(
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
        // الخطوة الأولى: إرسال الكود والتحقق من التكرار
        // ==========================================
        [HttpPost("register-step1-send-otp")]
        public async Task<IActionResult> RegisterStep1([FromBody] RegisterClientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string normalizedEmail = dto.Email.ToLower().Trim();

            // 1. التحقق هل هو مسجل كـ Client فعلاً في جدول العملاء؟
            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                var isAlreadyClient = await _context.Clients.AnyAsync(c => c.ApplicationUserId == existingUser.Id);
                if (isAlreadyClient)
                {
                    return BadRequest(new { message = "هذا البريد مسجل كعميل بالفعل، يمكنك تسجيل الدخول مباشرة" });
                }
            }

            // 2. توليد OTP مكون من 6 أرقام
            string otp = new Random().Next(100000, 999999).ToString();

            // 3. حفظ البيانات في الكاش (مفتاح خاص بالعملاء)
            var cacheEntry = new OtpCacheEntry<RegisterClientDto> { UserData = dto, OtpCode = otp };
            _cache.Set($"client_otp_{normalizedEmail}", cacheEntry, TimeSpan.FromMinutes(15));

            // 4. إرسال الإيميل
            try
            {
                _emailServices.SendEmail(new EmailDTO
                {
                    To = normalizedEmail,
                    Subject = "كود تفعيل حساب عميل - إتقان",
                    Body = $"<div style='direction:rtl; font-family:tahoma;'><h2>كود التفعيل الخاص بك هو: <span style='color:blue;'>{otp}</span></h2><p>هذا الكود صالح لمدة 15 دقيقة.</p></div>"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "فشل في إرسال البريد الإلكتروني، تأكد من الإعدادات", details = ex.Message });
            }

            return Ok(new { message = "تم إرسال كود التحقق بنجاح" });
        }

        // ==========================================
        // الخطوة الثانية: التحقق وإنشاء الحساب (Transaction)
        // ==========================================
        [HttpPost("register-step2-verify")]
        public async Task<IActionResult> RegisterStep2([FromBody] VerifyOtpDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string normalizedEmail = request.Email.ToLower().Trim();
            string cacheKey = $"client_otp_{normalizedEmail}";

            // 1. البحث في الكاش
            if (!_cache.TryGetValue(cacheKey, out OtpCacheEntry<RegisterClientDto> cachedData))
            {
                return BadRequest(new { message = "انتهت صلاحية الكود أو البريد غير موجود" });
            }

            // 2. مطابقة الكود
            if (cachedData.OtpCode != request.Otp)
            {
                return BadRequest(new { message = "كود التحقق غير صحيح" });
            }

            var dto = cachedData.UserData;

            // 3. بدء عملية الحفظ (Transaction) لضمان سلامة البيانات
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
                string userId;

                if (existingUser == null)
                {
                    // حالة أ: مستخدم جديد تماماً
                    var user = new ApplicationUser
                    {
                        FullName = dto.FullName,
                        Email = dto.Email,
                        UserName = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        Governorate = dto.Governorate,
                        UserType = UserType.Client,
                        EmailConfirmed = true // تم التأكيد بالـ OTP
                    };

                    var result = await _userManager.CreateAsync(user, dto.Password);
                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(result.Errors);
                    }
                    userId = user.Id;
                }
                else
                {
                    // حالة ب: مستخدم موجود (مثلاً حرفي) بيفتح حساب عميل بنفس الإيميل
                    userId = existingUser.Id;
                }

                // التأكد من وجود دور العميل (Role)
                if (!await _roleManager.RoleExistsAsync("Client"))
                    await _roleManager.CreateAsync(new IdentityRole("Client"));

                // إضافة الدور للمستخدم (لو مش مضاف له قبل كدة)
                if (!await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(userId), "Client"))
                {
                    await _userManager.AddToRoleAsync(await _userManager.FindByIdAsync(userId), "Client");
                }

                // إضافة سجل البروفايل في جدول الـ Clients
                var clientProfile = new Client
                {
                    Address = dto.Governorate,
                    ApplicationUserId = userId
                };

                _context.Clients.Add(clientProfile);
                await _context.SaveChangesAsync();

                // ثبت كل العمليات السابقة في الداتابيز
                await transaction.CommitAsync();

                // مسح الكاش بعد النجاح
                _cache.Remove(cacheKey);

                return Ok(new { message = "تم تفعيل حساب العميل بنجاح!" });
            }
            catch (Exception ex)
            {
                // لو حصل أي خطأ.. ارجع في كل اللي عملته (Rollback)
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "حدث خطأ فني أثناء حفظ البيانات", detail = ex.Message });
            }
        }
    }
}