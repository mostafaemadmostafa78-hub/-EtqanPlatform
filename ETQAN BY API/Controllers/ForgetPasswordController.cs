using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForgetPasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly IEmailServices _emailServices;

        public ForgetPasswordController(UserManager<ApplicationUser> userManager, IMemoryCache cache, IEmailServices emailServices)
        {
            _userManager = userManager;
            _cache = cache;
            _emailServices = emailServices;
        }
        [HttpPost("forgot-password-check-email")]
        public async Task<IActionResult> ForgotPasswordStep1(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest(new { message = "يرجى إدخال البريد الإلكتروني" });

            var user = await _userManager.FindByEmailAsync(email.ToLower().Trim());

            // 🛑 لو الإيميل مش موجود في الداتابيز
            if (user == null)
            {
                return BadRequest(new
                {
                    status = "not_found",
                    message = "ليس لديك حساب بهذا البريد، يرجى إنشاء حساب جديد أولاً"
                });
            }

            // ✅ لو موجود: ولد كود وابعته
            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"reset_otp_{email.ToLower().Trim()}", otp, TimeSpan.FromMinutes(60));

            _emailServices.SendEmail(new EmailDTO
            {
                To = email,
                Subject = "كود الدخول - إتقان",
                Body = $"<h2>كود التحقق الخاص بك هو: {otp}</h2>"
            });


            return Ok(new { message = "تم إرسال كود التحقق" });
        }
        // 1. لازم يكون عندك الـ DTO ده عشان يستلم الداتا
        public class VerifyOtpRequest
        {
            public string Email { get; set; }
            public string Otp { get; set; }
        }

        // 2. ده الأكشن اللي ناقصك ومسبب الـ 404
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            string email = request.Email.ToLower().Trim();
            string cacheKey = $"reset_otp_{email}";

            // بنشوف الكود موجود في الـ Cache ولا لأ
            if (!_cache.TryGetValue(cacheKey, out string cachedOtp))
            {
                return BadRequest(new { message = "انتهت صلاحية الكود أو غير موجود" });
            }

            // بنقارن الكود اللي بعته اليوزر بالكود اللي في السيرفر
            if (cachedOtp != request.Otp)
            {
                return BadRequest(new { message = "كود التحقق غير صحيح" });
            }

            // لو تمام
            return Ok(new { message = "الكود صحيح" });
        }
    }
}