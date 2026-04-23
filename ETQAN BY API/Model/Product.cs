using ETQAN_BY_API.Model;
using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Range(1, 100000)]
        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }
        public bool IsOffer { get; set; } = false;

        public string? UrlLImage { get; set; }
        public int StockQuantity { get; set; }

        // --- الربط مع الفئات (Category) ---
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // --- ⚡ الربط مع العلامات التجارية (Brand) ⚡ ---
        // بنضيف الـ Id والـ Navigation Property عشان الباك يفهم العلاقة
        public int BrandId { get; set; }
        public Brand Brand { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}