//using ETQAN.API.Data;
//using ETQAN.API.Models;
//using ETQAN_BY_API.DTO;
////using ETQAN_BY_API.DTO.Job;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace ETQAN_BY_API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class JobsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public JobsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var jobs = await _context.Jobs
//                .Select(j => new JobDto
//                {
//                    Id = j.Id,
//                    Name = j.Name
//                }).ToListAsync();

//            return Ok(jobs);
//        }
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetById(int id)
//        {
//            var job = await _context.Jobs.FindAsync(id);

//            if (job == null)
//                return NotFound();

//            return Ok(new JobDto
//            {
//                Id = job.Id,
//                Name = job.Name
//            });
//        }
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(int id, CreateJobDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var job = await _context.Jobs.FindAsync(id);
//            if (job == null)
//                return NotFound();

//            job.Name = dto.Name;

//            await _context.SaveChangesAsync();

//            return Ok("Job Updated");
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(CreateJobDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var job = new Job { Name = dto.Name };
//            _context.Jobs.Add(job);
//            await _context.SaveChangesAsync();

//            return Ok("Job Created");
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var job = await _context.Jobs.FindAsync(id);
//            if (job == null)
//                return NotFound();

//            _context.Jobs.Remove(job);
//            await _context.SaveChangesAsync();

//            return Ok("Job Deleted");
//        }
//    }
//}
