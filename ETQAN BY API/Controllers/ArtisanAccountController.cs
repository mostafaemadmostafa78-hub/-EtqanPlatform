using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[Route("api/[controller]")]
[ApiController]
public class ArtisanAccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMemoryCache _cache;
    private readonly IEmailServices _emailServices;

    public ArtisanAccountController(
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

    [HttpPost("register-step1-send-otp")]
    public async Task<IActionResult> RegisterStep1([FromBody] RegisterArtisanDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // توحيد شكل الإيميل (كل الحروف صغيرة)
        string normalizedEmail = dto.Email.ToLower().Trim();

        if (await _userManager.FindByEmailAsync(normalizedEmail) != null)
            return BadRequest(new { message = "هذا البريد الإلكتروني مسجل بالفعل" });

        // توليد الكود وحفظه
        string otp = new Random().Next(100000, 999999).ToString();
        var cacheEntry = new OtpCacheEntry { UserData = dto, OtpCode = otp };

        // حفظ في الكاش لمدة 15 دقيقة (زودنا الوقت قليلاً)
        _cache.Set(normalizedEmail, cacheEntry, TimeSpan.FromMinutes(15));

        _emailServices.SendEmail(new EmailDTO
        {
            To = normalizedEmail,
            Subject = "كود تفعيل حساب إتقان",
            Body = $"<h2>كود التفعيل: {otp}</h2>"
        });

        return Ok(new { message = "تم إرسال الكود بنجاح" });
    }

    
    [HttpPost("register-step2-verify")]
    public async Task<IActionResult> RegisterStep2([FromBody] VerifyOtpDto request)
    {
        // أضف سطر للـ Debug عشان تشوف الإيميل اللي جاي لك في الـ Console بتاع الـ C#
        Console.WriteLine($"Attempting to verify: {request.Email} with OTP: {request.Otp}");

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
            return BadRequest(new { message = "البيانات ناقصة" });

        string normalizedEmail = request.Email.ToLower().Trim();

        if (!_cache.TryGetValue(normalizedEmail, out OtpCacheEntry cachedData))
        {
            return BadRequest(new { message = "انتهت صلاحية الكود أو الإيميل غير موجود" });
        }

        if (cachedData.OtpCode.Trim() != request.Otp.Trim())
        {
            return BadRequest(new { message = "كود التحقق غير صحيح" });
        }

        var dto = cachedData.UserData;

        // تنفيذ عملية الحفظ (نفس منطق كودك الأصلي)
        var user = new ApplicationUser
        {
            FullName = dto.Fullname,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.phoneNumber,
            UserType = UserType.Artisan,
           // Governorate = dto.Governorate,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        if (!await _roleManager.RoleExistsAsync("Artisan"))
            await _roleManager.CreateAsync(new IdentityRole("Artisan"));
        await _userManager.AddToRoleAsync(user, "Artisan");

        var artisan = new Artisan
        {
            Age = dto.Age,
            NationalId = dto.NationalId,
            MaritalStatus = (MaritalStatus)dto.MaritalStatus,
            JobId = dto.JobId,
            ApplicationUserId = user.Id,
           // StartingPrice = dto.StartingPrice,
            
            
        };

        _context.Artisans.Add(artisan);
        await _context.SaveChangesAsync();

        _cache.Remove(normalizedEmail); // مسح الكاش

        return Ok(new { message = "تم إنشاء الحساب بنجاح!" });
    }
}