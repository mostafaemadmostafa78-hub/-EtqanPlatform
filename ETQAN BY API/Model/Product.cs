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

        // ✅ أضف السطرين دول
        public decimal OldPrice { get; set; }
        public bool IsOffer { get; set; } = false;

        public string? UrlLImage { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}