using ETQAN.API.Data;
using ETQAN.API.Models;
using ETQAN_BY_API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ✅ ميثود مساعدة (Private) عشان نمنع تكرار كود الـ Mapping
    private IQueryable<ProductDto> MapToDto(IQueryable<Product> query)
    {
        return query.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            OldPrice = p.OldPrice,
            StockQuantity = p.StockQuantity,
            CategoryName = p.Category.Name,
            UrlImage = p.UrlLImage,
            IsOffer = p.IsOffer,
            Discount = (p.OldPrice > 0 && p.OldPrice > p.Price)
                ? $"{(int)((1 - (double)p.Price / (double)p.OldPrice) * 100)}%"
                : "0%"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = _context.Products.Include(p => p.Category);
        return Ok(await MapToDto(query).ToListAsync());
    }

    [HttpGet("best-sellers")]
    public async Task<IActionResult> GetBestSellers()
    {
        var query = _context.Products.Include(p => p.Category).Where(p => !p.IsOffer);
        return Ok(await MapToDto(query).ToListAsync());
    }

    [HttpGet("offers")]
    public async Task<IActionResult> GetOffers()
    {
        var query = _context.Products.Include(p => p.Category).Where(p => p.IsOffer);
        return Ok(await MapToDto(query).ToListAsync());
    }

    // 🔍 إضافة ميزة البحث (Search)
    [HttpGet("search")]
    public async Task<IActionResult> Search(string name)
    {
        var query = _context.Products.Include(p => p.Category)
            .Where(p => p.Name.Contains(name));
        return Ok(await MapToDto(query).ToListAsync());
    }

    // 📂 إضافة فلترة حسب القسم (Filter by Category)
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var query = _context.Products.Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId);
        return Ok(await MapToDto(query).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = _context.Products.Include(p => p.Category).Where(p => p.Id == id);
        var product = await MapToDto(query).FirstOrDefaultAsync();

        if (product == null)
            return NotFound(new { message = "المنتج غير موجود" });

        return Ok(product);
    }

    //  [Authorize(Roles = "Admin")] // 🔐 للأدمن فقط
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists) return BadRequest(new { message = "الفئة غير موجودة" });

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            OldPrice = dto.OldPrice,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            UrlLImage = dto.UrlImage,
            IsOffer = dto.IsOffer
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, "تم إضافة المنتج بنجاح");
    }

    // [Authorize(Roles = "Admin")]
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
        product.UrlLImage = dto.UrlImage;
        product.IsOffer = dto.IsOffer;

        await _context.SaveChangesAsync();
        return Ok(new { message = "تم التحديث بنجاح" });
    }

    //[Authorize(Roles = "Admin")]
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