using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETQAN.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 🛠️ ميثود مساعدة لتوحيد شكل البيانات (Projection)
        // ==========================================
        private IQueryable<ProductDto> MapToDto(IQueryable<Product> query)
        {
            return query.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OldPrice = p.OldPrice,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.Name,
                BrandName = p.Brand.Name, // أضفنا البراند هنا للـ DTO
                UrlImage = p.UrlLImage,
                IsOffer = p.IsOffer,
                Discount = (p.OldPrice > p.Price && p.OldPrice > 0)
                    ? $"{(int)((1 - (double)p.Price / (double)p.OldPrice) * 100)}%"
                    : "0%"
            });
        }

        // ==========================================
        // 🔍 عمليات القراءة (GET Methods)
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Brand);
            return Ok(await MapToDto(query).ToListAsync());
        }

        [HttpGet("best-sellers")]
        public async Task<IActionResult> GetBestSellers()
        {
            var query = _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => !p.IsOffer);
            return Ok(await MapToDto(query).ToListAsync());
        }

        [HttpGet("offers")]
        public async Task<IActionResult> GetOffers()
        {
            var query = _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsOffer);
            return Ok(await MapToDto(query).ToListAsync());
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest(new { message = "يرجى إدخال نص للبحث" });

            var query = _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Name.Contains(name));

            return Ok(await MapToDto(query).ToListAsync());
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var query = _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == categoryId);

            return Ok(await MapToDto(query).ToListAsync());
        }

        [HttpGet("brand/{brandName}")]
        public async Task<IActionResult> GetByBrand(string brandName)
        {
            var brand = await _context.Brands.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Name == brandName);

            if (brand == null) return NotFound(new { message = "هذه العلامة التجارية غير موجودة" });

            var query = _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.BrandId == brand.Id);

            return Ok(new
            {
                brandHeader = brand.UrlImage,
                products = await MapToDto(query).ToListAsync()
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Id == id);

            var product = await MapToDto(query).FirstOrDefaultAsync();

            if (product == null) return NotFound(new { message = "المنتج غير موجود" });

            return Ok(product);
        }

        // ==========================================
        // 💾 عمليات التعديل (CRUD Methods)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. التأكد إن الفئة موجودة
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists) return BadRequest(new { message = "الفئة المحددة غير موجودة" });

            // 2. التأكد إن البراند موجود (هنا مكان المشكلة اللي حصلت معاك)
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == dto.BrandId);
            if (!brandExists) return BadRequest(new { message = "العلامة التجارية المحددة غير موجودة.. تأكد من الـ Id" });

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                OldPrice = dto.OldPrice,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                UrlLImage = dto.UrlImage,
                IsOffer = dto.IsOffer
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { message = "تمت إضافة المنتج بنجاح", id = product.Id });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { message = "المنتج غير موجود" });

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.OldPrice = dto.OldPrice;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.UrlLImage = dto.UrlImage;
            product.IsOffer = dto.IsOffer;

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث بيانات المنتج بنجاح" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { message = "المنتج غير موجود" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حذف المنتج بنجاح" });
        }
    }
}