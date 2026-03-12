using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ETQAN_BY_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] ReviewCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var artisanExists = await _context.Artisans.AnyAsync(a => a.Id == dto.ArtisanId);
            if (!artisanExists)
                return NotFound(new { Message = "Artisan not found." });

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ArtisanId == dto.ArtisanId && r.ReviewerId == userId);

            if (alreadyReviewed)
                return BadRequest(new { Message = "You have already reviewed this artisan." });

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                ArtisanId = dto.ArtisanId,
                ReviewerId = userId,
               // CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var response = new ReviewReadOnlyDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewerId = review.ReviewerId,
                //CreatedAt = review.CreatedAt
            };

            return CreatedAtAction(nameof(GetArtisanReviews), new { artisanId = review.ArtisanId }, response);
        }

        [AllowAnonymous]
        [HttpGet("{artisanId}")]
        public async Task<IActionResult> GetArtisanReviews(int artisanId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Page number and size must be greater than 0.");

            var reviewsQuery = _context.Reviews
                .AsNoTracking()
                .Where(r => r.ArtisanId == artisanId);
               // .OrderByDescending(r => r.CreatedAt);

            var totalItems = await reviewsQuery.CountAsync();
            var reviews = await reviewsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewReadOnlyDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewerId = r.ReviewerId,
                   // CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            if (!reviews.Any())
                return NotFound(new { Message = "No reviews found for this artisan." });

            return Ok(new
            {
                TotalCount = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = reviews
            });
        }
    }
}