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

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtisansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        //ah
        private readonly IArtisanService _artisanService;
        private readonly UserManager<ApplicationUser> _userManager;
        public ArtisansController(ApplicationDbContext context, IArtisanService artisanService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _artisanService = artisanService; 
            _userManager = userManager;
            _userManager = userManager;
        }
        //.

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Artisans.Include(a => a.User).Include(a => a.Job).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.User.FullName.Contains(search) || a.Job.Name.Contains(search));
            }

            var result = await query.Select(a => new ArtisanListDto
            {
                Id = a.ApplicationUserId,
                Name = a.User.FullName,
                JobName = a.Job.Name,
                Rating = 4.8,
                ImageUrl = a.User.ProfilePicture ?? "/images/Artisans/default.svg"
            }).ToListAsync();

            return Ok(result);
        }


//ah
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {

                var result = await _artisanService.GetArtisanDetailsAsync(id);

                if (result == null)
                {
                    return NotFound(new { message = "هذا الحرفي غير موجود" });
                }

                return Ok(result);
               
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب البيانات", error = ex.Message });
            }
        }

        //.



        //ah
        [Authorize(Roles = "Artisan")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateArtisanProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "يجب تسجيل الدخول أولاً" });

            try
            {
                var result = await _artisanService.UpdateArtisanProfileAsync(userId, dto);
                if (!result) return BadRequest(new { message = "حدث خطأ أثناء تحديث البيانات" });

                return Ok(new { message = "تم تحديث الملف الشخصي بنجاح" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Artisan")]
        [HttpPost("change-password")]
        public async Task<IActionResult> UpdateProfile([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId); 

            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "تم تغيير كلمة السر بنجاح" });
        }


        [Authorize(Roles = "Artisan")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtisan(string id)
        {
            var result = await _artisanService.DeleteArtisanAsync(id);
            if (!result) return NotFound("فشل الحذف، الحرفي غير موجود");

            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }

        [Authorize(Roles = "Artisan")]
        [HttpPost("portfolio/add")]
        public async Task<IActionResult> AddPortfolioImage([FromForm] AddPortfolioImageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (dto.Image == null) return BadRequest("يرجى اختيار صورة أولاً");

            var result = await _artisanService.AddImageToPortfolioAsync(userId, dto);

            if (result) return Ok(new { message = "تم إضافة الصورة لمعرض أعمالك بنجاح" });

            return BadRequest("حدث خطأ أثناء رفع الصورة");
        }

        [Authorize(Roles = "Artisan")]
        [HttpDelete("portfolio/delete/{imageId}")]
        public async Task<IActionResult> DeletePortfolioImage(int imageId)
        {
            var result = await _artisanService.DeleteImageFromPortfolioAsync(imageId);

            if (result) return Ok(new { message = "تم حذف الصورة بنجاح" });

            return NotFound("الصورة غير موجودة أو تم حذفها بالفعل");
        }
     
        //.
        
    }
}