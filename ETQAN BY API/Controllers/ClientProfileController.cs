using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client")]
public class ClientProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IClientService _clientService;

    public ClientProfileController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IClientService clientService)
    {
        _context = context;
        _userManager = userManager;
        _clientService = clientService;
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetProfileInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return NotFound();

        return Ok(new
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Governorate = user.Governorate ?? "غير محدد",
            ProfilePicture = user.ProfilePicture ?? "/images/default-user.png",
            CoverPicture = user.CoverPicture ?? "/images/default-cover.png"
        });
    }
    //mo
    //[HttpGet("history")]
    //public async Task<IActionResult> GetHistory()
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    var history = await _context.ServiceRequests
    //        .Include(r => r.Artisan).ThenInclude(a => a.User)
    //        .Include(r => r.Artisan).ThenInclude(a => a.Job)
    //        .Where(r => r.Client.ApplicationUserId == userId)
    //        .OrderByDescending(r => r.RequestDate)
    //        .Select(r => new ClientHistoryDto
    //        {
    //            RequestId = r.Id,
    //            ArtisanName = r.Artisan != null ? r.Artisan.User.FullName : "جاري البحث..",
    //            JobName = r.Artisan != null ? r.Artisan.Job.Name : r.ServiceName,
    //            ArtisanImage = r.Artisan != null ? (r.Artisan.User.ProfilePicture ?? "/images/default.svg") : "/images/default.svg",
    //            Status = r.Status.ToString()
    //        }).ToListAsync();

    //    return Ok(history);
    //}

    //mo
    //[HttpGet("my-reviews")]
    //public async Task<IActionResult> GetMyReviews()
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    var reviews = await _context.Reviews
    //        .Include(r => r.Artisan).ThenInclude(a => a.User)
    //        .Include(r => r.Artisan).ThenInclude(a => a.Job)
    //        .Where(r => r.ReviewerId == userId)
    //        .Select(r => new ClientReviewDto
    //        {
    //            ReviewId = r.Id,
    //            ArtisanName = r.Artisan.User.FullName,
    //            ArtisanJob = r.Artisan.Job.Name,
    //            ArtisanImage = r.Artisan.User.ProfilePicture ?? "/images/default.svg",
    //            Rating = r.Rating,
    //            Comment = r.Comment,
    //            Date = DateTime.Now.ToShortDateString()
    //        }).ToListAsync();

    //    return Ok(reviews);
    //}


    //mo  //[HttpPut("update")]
    //public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    var user = await _userManager.FindByIdAsync(userId);

    //    if (user == null) return NotFound();

    //    user.Email = dto.Email;
    //    user.PhoneNumber = dto.PhoneNumber;
    //    user.Governorate = dto.Governorate;

    //    if (!string.IsNullOrEmpty(dto.NewPassword))
    //    {
    //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    //        await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
    //    }

    //    var result = await _userManager.UpdateAsync(user);
    //    if (result.Succeeded) return Ok(new { message = "تم تحديث البيانات بنجاح" });

    //    return BadRequest(result.Errors);
    //}

    //ah
    [Authorize(Roles = "Client")]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] ClientProfileUpdateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _clientService.UpdateProfileComprehensiveAsync(userId, dto);

        if (result)
            return Ok(new { message = "تم تحديث البيانات بنجاح" });

        return BadRequest("حدث خطأ أثناء تحديث الملف الشخصي");
    }

    [Authorize(Roles = "Client")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { message = "تم تغيير كلمة السر بنجاح" });
    }

    //ah
    [Authorize(Roles = "Client")]
    [HttpDelete("delete-my-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _clientService.DeleteClientAccountAsync(userId);

        if (result) return Ok(new { message = "تم حذف الحساب بنجاح" });
        return BadRequest("فشل حذف الحساب");
    }
    //.

    //ah - تعديل تقييم العميل لعمله السابق
    [Authorize(Roles = "Client")]
    [HttpPut("review/{reviewId}")]
    public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto dto)
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _clientService.UpdateReviewAsync(reviewId, clientId, dto);

        if (!result) return BadRequest("تعذر تعديل التقييم أو التقييم غير موجود");
        return Ok("تم تحديث التقييم بنجاح");
    }

    //ah - حذف تقييم
    [Authorize(Roles = "Client")]
    [HttpDelete("review/{reviewId}")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _clientService.DeleteReviewAsync(reviewId, clientId);

        if (!result) return BadRequest("تعذر حذف التقييم");
        return Ok("تم حذف التقييم بنجاح");
    }
    //.
    //ah
    [Authorize(Roles = "Client")]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] string? status = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var history = await _clientService.GetClientHistoryAsync(userId, status);
        return Ok(history);
    }
    //.

    [HttpGet("my-reviews")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reviews = await _clientService.GetMyReviewsAsync(userId);
        return Ok(reviews);
    }
}