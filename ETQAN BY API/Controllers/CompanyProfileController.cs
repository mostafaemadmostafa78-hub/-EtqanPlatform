using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN.API.Models.Enums;
using ETQAN_BY_API.DTO;
using ETQAN_BY_API.Model;
using ETQAN_BY_API.Model.DTOs;
using ETQAN_BY_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;

        public CompanyProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
        }

        [HttpGet("my-reviews")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (company == null) return NotFound("الشركة غير موجودة");

            var reviews = await _context.Reviews
                .Where(r => r.CompanyId == company.Id)
                .Include(r => r.Client).ThenInclude(cl => cl.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new {
                    CustomerName = r.Client.User.FullName,
                    CustomerImage = r.Client.User.ProfilePicture,
                    Rating = r.Rating.ToString("0.0"),
                    Comment = r.Comment,
                    Date = r.CreatedAt.ToString("yyyy/MM/dd"),
                    TimeAgo = CalculateTimeAgo(r.CreatedAt)
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("{companyId}/public-profile")]
        public async Task<IActionResult> GetCompanyPublicProfile(int companyId)
        {
            var company = await _context.Companies
                .Include(c => c.User)
                .Include(c => c.Portfolio)
                .Include(c => c.Reviews).ThenInclude(r => r.Client).ThenInclude(cl => cl.User)
                .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);

            if (company == null) return NotFound("الشركة غير موجودة أو تم حذف الحساب.");

            var averageRating = company.Reviews != null && company.Reviews.Any()
                                ? Math.Round(company.Reviews.Average(r => r.Rating), 1)
                                : 0;

            var result = new
            {
                company.Id,
                company.CompanyName,
                company.Description,
                company.ServiceDetails,
                company.ExperienceYears,
                company.WorkingHours,
                company.User.Governorate,
                ProfilePicture = company.User.ProfilePicture,
                CoverPhoto = company.CoverPhoto,
                Portfolio = company.Portfolio.Select(p => p.ImageUrl),
                AverageRating = averageRating,
                TotalReviews = company.Reviews?.Count() ?? 0,

                Reviews = company.Reviews.OrderByDescending(r => r.CreatedAt).Select(r => new
                {
                    r.Id,
                    Rating = r.Rating.ToString("0.0"),
                    r.Comment,
                    ClientName = r.Client.User.FullName,
                    ClientImage = r.Client.User.ProfilePicture,
                    TimeAgo = CalculateTimeAgo(r.CreatedAt)
                }).ToList()
            };

            return Ok(result);
        }

        [HttpGet("filter-by-service/{jobId}")]
        public async Task<IActionResult> GetCompaniesByService(int jobId)
        {
            var jobIdStr = jobId.ToString();
            var companies = await _context.Companies
                .Include(c => c.User)
                .Include(c => c.Reviews)
                .Where(c => c.ServiceIds != null && c.ServiceIds.Contains(jobIdStr))
                .Select(c => new
                {
                    c.Id,
                    c.CompanyName,
                    c.User.ProfilePicture,
                    Rating = c.Reviews.Any() ? (double)Math.Round(c.Reviews.Average(r => (double)r.Rating), 1) : 0.0,
                })
                .ToListAsync();

            if (!companies.Any())
                return NotFound(new { message = "لا توجد شركات تقدم هذه الخدمة حالياً" });

            return Ok(companies);
        }

        [HttpGet("search-by-name")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            var companies = await _context.Companies
                .Include(c => c.User)
                .Include(c => c.Reviews)
                .Where(c => c.CompanyName.Contains(name))
                .Select(c => new
                {
                    c.Id,
                    c.CompanyName,
                    c.User.ProfilePicture,
                    Rating = c.Reviews.Any()
                       ? double.Parse(c.Reviews.Average(r => (double)r.Rating).ToString("0.0"))
                          : 0.0
                })
                .ToListAsync();

            return Ok(companies);
        }

        [Authorize(Roles = "Company")]
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var company = await _context.Companies
                .Include(c => c.User)
                .Include(c => c.Portfolio)
                .Include(c => c.Reviews).ThenInclude(r => r.Client).ThenInclude(cl => cl.User)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (company == null) return NotFound();

            var averageRating = company.Reviews != null && company.Reviews.Any()
                                ? Math.Round(company.Reviews.Average(r => r.Rating), 1)
                                : 0;

            var completedOrdersCount = await _context.ServiceRequests
                .CountAsync(r => r.CompanyId == company.Id && r.Status == RequestStatus.Finished);

            var result = new
            {
                company.CompanyName,
                company.Description,
                company.ServiceDetails,
                company.ExperienceYears,
                company.WorkingHours,
                company.User.Governorate,
                Email = company.User.Email,
                Phone = company.User.PhoneNumber,
                Portfolio = company.Portfolio.Select(p => p.ImageUrl),
                AverageRating = averageRating,
                CompletedOrdersCount = completedOrdersCount,
                JoinedDate = company.User.CreatedAt.ToString("d/M/yyyy"),
                TotalReviews = company.Reviews?.Count() ?? 0,

                Reviews = company.Reviews.Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    ClientName = r.Client.User.FullName,
                    ClientImage = r.Client.User.ProfilePicture,
                    TimeAgo = CalculateTimeAgo(r.CreatedAt)
                }).ToList(),

                RatingStats = new
                {
                    FiveStars = company.Reviews.Count(r => r.Rating == 5),
                    FourStars = company.Reviews.Count(r => r.Rating == 4),
                    ThreeStars = company.Reviews.Count(r => r.Rating == 3),
                    TwoStars = company.Reviews.Count(r => r.Rating == 2),
                    OneStar = company.Reviews.Count(r => r.Rating == 1),
                    TotalReviews = company.Reviews.Count()
                }
            };

            return Ok(result);
        }

        [Authorize(Roles = "Client")]
        [HttpPost("add-company-review")]
        public async Task<IActionResult> AddOrUpdateCompanyReview([FromBody] CompanyReviewCreateDto dto)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ApplicationUserId == clientId);
            if (client == null) return BadRequest("العميل غير موجود في جدول Clients");

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.ClientId == clientId && o.CompanyId != null);

            if (serviceRequest == null) return NotFound("الطلب غير موجود");

            if (serviceRequest.Status != RequestStatus.Finished)
            {
                return BadRequest("يجب إنهاء الخدمة أولاً قبل تقييم الشركة.");
            }

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ServiceRequestId == dto.OrderId && r.CompanyId != null);

            if (existingReview != null)
            {
                if ((DateTime.Now - existingReview.CreatedAt).TotalHours > 24)
                {
                    return BadRequest("انتهت مهلة الـ 24 ساعة المسموح بها لتعديل التقييم.");
                }
                existingReview.Rating = dto.Rating;
                existingReview.Comment = dto.Comment;
                existingReview.CreatedAt = DateTime.Now;
            }
            else
            {
                _context.Reviews.Add(new Review
                {
                    ServiceRequestId = serviceRequest.Id,
                    CompanyId = serviceRequest.CompanyId,
                    ReviewerId = clientId,
                    ClientId = client.Id,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.Now,
                    OrderId = _context.Orders.FirstOrDefault(o => o.ServiceRequestId == serviceRequest.Id)?.Id
                });
            }

            var saved = await _context.SaveChangesAsync() > 0;
            if (saved)
            {
                await UpdateCompanyAverageRating(serviceRequest.CompanyId.Value);
            }

            return Ok(new { message = "تم حفظ التقييم بنجاح" });
        }

        private async Task UpdateCompanyAverageRating(int companyId)
        {
            var company = await _context.Companies
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company != null)
            {
                company.AverageRating = company.Reviews.Any()
                    ? (decimal)Math.Round(company.Reviews.Average(r => (double)r.Rating), 1)
                    : 0;

                company.CompletedOrdersCount = await _context.ServiceRequests
                    .CountAsync(r => r.CompanyId == company.Id && r.Status == RequestStatus.Finished);

                await _context.SaveChangesAsync();
            }
        }

        [Authorize(Roles = "Company")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] CompanyProfileUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var company = await _context.Companies
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (company == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Email))
                company.User.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Phone))
                company.User.PhoneNumber = dto.Phone;

            if (!string.IsNullOrEmpty(dto.Governorate))
                company.User.Governorate = dto.Governorate;

            if (!string.IsNullOrEmpty(dto.Description))
                company.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.ServiceDetails))
                company.ServiceDetails = dto.ServiceDetails;

            if (dto.ExperienceYears.HasValue)
                company.ExperienceYears = dto.ExperienceYears.Value;

            if (!string.IsNullOrEmpty(dto.WorkingHours))
                company.WorkingHours = dto.WorkingHours;

            if (!string.IsNullOrEmpty(dto.ServiceArea))
                company.ServiceArea = dto.ServiceArea;

            if (!string.IsNullOrEmpty(dto.ResponseTime))
                company.ResponseTime = dto.ResponseTime;

            if (!string.IsNullOrEmpty(dto.EmergencyService))
                company.IsEmergencyAvailable = dto.EmergencyService == "نعم";

            if (dto.ProfilePictureFile != null)
            {
                if (!string.IsNullOrEmpty(company.User.ProfilePicture))
                    _fileService.DeleteImage(company.User.ProfilePicture);

                company.User.ProfilePicture = await _fileService.UploadImageAsync(dto.ProfilePictureFile, "uploads/profiles/companies");
            }

            if (dto.CoverPhotoFile != null)
            {
                if (!string.IsNullOrEmpty(company.CoverPhoto))
                    _fileService.DeleteImage(company.CoverPhoto);

                company.CoverPhoto = await _fileService.UploadImageAsync(dto.CoverPhotoFile, "images/covers/companies");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث الملف الشخصي بنجاح" });
        }

        [Authorize(Roles = "Company")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { message = "تم تغيير كلمة السر بنجاح" });
        }

        [Authorize(Roles = "Company")]
        [HttpPost("add-portfolio-image")]
        public async Task<IActionResult> AddPortfolioImage([FromForm] AddPortfolioImageCompanyDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (company == null) return NotFound(new { message = "الشركة غير موجودة" });
            if (dto.ImageFile == null || dto.ImageFile.Length == 0)
                return BadRequest(new { message = "يرجى اختيار صورة صالحة" });

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/portfolio");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ImageFile.CopyToAsync(stream);
            }

            var portfolioEntry = new CompanyPortfolio
            {
                ImageUrl = "/uploads/portfolio/" + fileName,
                Description = dto.Description,
                CompanyId = company.Id
            };
            _context.CompanyPortfolios.Add(portfolioEntry);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تمت إضافة العمل بنجاح", imageUrl = portfolioEntry.ImageUrl });
        }

        [Authorize(Roles = "Company")]
        [HttpDelete("delete-portfolio-image/{id}")]
        public async Task<IActionResult> DeletePortfolioImage(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            var image = await _context.CompanyPortfolios.FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == company.Id);
            if (image == null) return NotFound(new { message = "الصورة غير موجودة أو لا تملك صلاحية حذفها" });

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

            _context.CompanyPortfolios.Remove(image);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حذف الصورة بنجاح" });
        }

        private string CalculateTimeAgo(DateTime dateTime)
        {
            var timespan = DateTime.Now - dateTime;
            if (timespan.TotalDays > 30) return dateTime.ToString("yyyy/MM/dd");
            if (timespan.TotalDays >= 1) return $"منذ {Math.Floor(timespan.TotalDays)} يوم";
            if (timespan.TotalHours >= 1) return $"منذ {Math.Floor(timespan.TotalHours)} ساعة";
            return "الآن";
        }

        [Authorize(Roles = "Company")]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (company == null)
                return NotFound(new { message = "الحساب غير موجود" });

            company.IsDeleted = true;

            var user = await _userManager.FindByIdAsync(company.ApplicationUserId);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                await _userManager.UpdateSecurityStampAsync(user);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }
    }
}