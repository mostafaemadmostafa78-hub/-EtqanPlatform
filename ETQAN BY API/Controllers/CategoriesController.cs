//using ETQAN.API.Data;
//using ETQAN.API.Models;
//using ETQAN_BY_API.DTO.CategoryDTO;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace ETQAN_BY_API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class CategoriesController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public CategoriesController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var categories = await _context.Categories
//                .Select(c => new CategoryDto
//                {
//                    Id = c.Id,
//                    Name = c.Name
//                }).ToListAsync();

//            return Ok(categories);
//        }
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var category = await _context.Categories.FindAsync(id);
//            if (category == null)
//                return NotFound();

//            category.Name = dto.Name;

//            await _context.SaveChangesAsync();

//            return Ok("Category Updated");
//        }
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetById(int id)
//        {
//            var category = await _context.Categories.FindAsync(id);

//            if (category == null)
//                return NotFound();

//            return Ok(new CategoryDto
//            {
//                Id = category.Id,
//                Name = category.Name
//            });
//        }
//        [HttpPost]
//        public async Task<IActionResult> Create(CreateCategoryDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var category = new Category { Name = dto.Name };
//            _context.Categories.Add(category);
//            await _context.SaveChangesAsync();

//            return Ok("Category Created");
//        }
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var category = await _context.Categories.FindAsync(id);
//            if (category == null)
//                return NotFound();

//            _context.Categories.Remove(category);
//            await _context.SaveChangesAsync();

//            return Ok("Category Deleted");
//        }
//    }
//}
