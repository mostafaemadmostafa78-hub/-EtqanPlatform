using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
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

        return Ok(new ClientProfileDto
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Governorate = user.Governorate ?? "غير محدد",
            ProfilePicture = user.ProfilePicture ?? "/images/Artisans/default.svg"
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var history = await _context.ServiceRequests
            .Include(r => r.Artisan).ThenInclude(a => a.User)
            .Include(r => r.Artisan).ThenInclude(a => a.Job)
            .Where(r => r.Client.ApplicationUserId == userId)
            .OrderByDescending(r => r.RequestDate)
            .Select(r => new ClientHistoryDto
            {
                RequestId = r.Id,
                ArtisanName = r.Artisan != null ? r.Artisan.User.FullName : "جاري البحث..",
                JobName = r.Artisan != null ? r.Artisan.Job.Name : r.ServiceName,
                ArtisanImage = r.Artisan != null ? (r.Artisan.User.ProfilePicture ?? "/images/default.svg") : "/images/default.svg",
                Status = r.Status.ToString()
            }).ToListAsync();

        return Ok(history);
    }

    [HttpGet("my-reviews")]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var reviews = await _context.Reviews
            .Include(r => r.Artisan).ThenInclude(a => a.User)
            .Include(r => r.Artisan).ThenInclude(a => a.Job)
            .Where(r => r.ReviewerId == userId)
            .Select(r => new ClientReviewDto
            {
                ReviewId = r.Id,
                ArtisanName = r.Artisan.User.FullName,
                ArtisanJob = r.Artisan.Job.Name,
                ArtisanImage = r.Artisan.User.ProfilePicture ?? "/images/default.svg",
                Rating = r.Rating,
                Comment = r.Comment,
                Date = DateTime.Now.ToShortDateString()
            }).ToListAsync();

        return Ok(reviews);
    }

    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return NotFound();

        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.Governorate = dto.Governorate;

        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return Ok(new { message = "تم تحديث البيانات بنجاح" });

        return BadRequest(result.Errors);
    }

//ah
    [HttpDelete("delete-my-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _clientService.DeleteClientAccountAsync(userId);

        if (result) return Ok(new { message = "تم حذف الحساب بنجاح" });
        return BadRequest("فشل حذف الحساب");
    }
    //.
}