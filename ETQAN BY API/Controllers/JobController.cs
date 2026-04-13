using ETQAN.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public JobsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs()
    {
        // بنجيب الوظائف مرتبة أبجدياً
        var jobs = await _context.Jobs
            .Select(j => new { j.Id, j.Name })
            .OrderBy(j => j.Name)
            .ToListAsync();

        return Ok(jobs);
    }
    [HttpGet("by-job/{jobId}")]
    public async Task<IActionResult> GetArtisansByJob(int jobId)
    {
        var artisans = await _context.Artisans
            .Include(a => a.User) // عشان نجيب الاسم من الـ ApplicationUser
            .Where(a => a.JobId == jobId)
            .Select(a => new {
                a.Id,
                fullName = a.User.FullName,
                jobName = a.Job.Name,
                startingPrice = a.StartingPrice,
                // أي بيانات تانية عايز تعرضها في الكارت
            })
            .ToListAsync();

        return Ok(artisans);
    }
}