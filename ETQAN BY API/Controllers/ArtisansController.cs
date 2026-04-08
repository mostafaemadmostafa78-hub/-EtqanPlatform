using ETQAN.API.Data;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model.DTOs;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Authorization;
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

        public ArtisansController(ApplicationDbContext context, IArtisanService artisanService)
        {
            _context = context;
            _artisanService = artisanService; // هنا بنربط الخدمة
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
                Price = a.StartingPrice,
                Rating = 4.8,
                ImageUrl = a.User.ProfilePicture ?? "/images/Artisans/default.svg"
            }).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var artisan = await _context.Artisans
                    .Include(a => a.User)
                    .Include(a => a.Job)
                    .Include(a => a.Portfolio)
                    .FirstOrDefaultAsync(a => a.ApplicationUserId == id);

                if (artisan == null)
                {
                    return NotFound(new { message = "هذا الحرفي غير موجود" });
                }

                var result = new ArtisanDetailsDto
                {
                    Id = artisan.ApplicationUserId,
                    FullName = artisan.User.FullName,
                    JobName = artisan.Job?.Name ?? "حرفي",
                    Bio = artisan.Bio ?? "لا يوجد نبذة تعريفية حالياً",
                    ExperienceYears = artisan.ExperienceYears,
                    StartingPrice = artisan.StartingPrice,
                    Governorate = artisan.User.Governorate ?? "غير محددة",
                    Rating = 4.8,
                    ProfilePicture = artisan.User.ProfilePicture ?? "/images/Artisans/default.svg",
                    PortfolioImages = artisan.Portfolio != null
                        ? artisan.Portfolio.Select(p => p.ImageUrl).ToList()
                        : new List<string>()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب بيانات البروفايل", error = ex.Message });
            }
        }

        //ah
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateArtisanProfile([FromBody] UpdateArtisanProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "يجب تسجيل الدخول أولاً" });

            var result = await _artisanService.UpdateArtisanProfileAsync(userId, dto);

            if (!result)
                return BadRequest(new { message = "حدث خطأ أثناء تحديث البيانات" });

            return Ok(new { message = "تم تحديث الملف الشخصي بنجاح" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArtisan(string id)
        {
            var result = await _artisanService.DeleteArtisanAsync(id);
            if (!result) return NotFound("فشل الحذف، الحرفي غير موجود");

            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }

        [HttpPost("portfolio/add")]
        [Authorize]
        public async Task<IActionResult> AddPortfolioImage([FromForm] AddPortfolioImageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (dto.Image == null) return BadRequest("يرجى اختيار صورة أولاً");

            var result = await _artisanService.AddImageToPortfolioAsync(userId, dto);

            if (result) return Ok(new { message = "تم إضافة الصورة لمعرض أعمالك بنجاح" });

            return BadRequest("حدث خطأ أثناء رفع الصورة");
        }

        [HttpDelete("portfolio/delete/{imageId}")]
        [Authorize]
        public async Task<IActionResult> DeletePortfolioImage(int imageId)
        {
            var result = await _artisanService.DeleteImageFromPortfolioAsync(imageId);

            if (result) return Ok(new { message = "تم حذف الصورة بنجاح" });

            return NotFound("الصورة غير موجودة أو تم حذفها بالفعل");
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "Artisan")]
        public async Task<IActionResult> GetMyOrders()
        {
            var artisanId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (artisanId == null) return Unauthorized();

            var orders = await _artisanService.GetMyOrdersAsync(artisanId);
            return Ok(orders);
        }

        [HttpPut("orders/{orderId}/status")]
        [Authorize(Roles = "Artisan")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromQuery] string newStatus)
        {
            var result = await _artisanService.UpdateOrderStatusAsync(orderId, newStatus);

            if (result)
                return Ok(new { message = $"تم تحديث حالة الطلب إلى {newStatus} بنجاح" });

            return BadRequest("فشل تحديث الحالة");
        }
        //.
    }
}