using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class ProductDto
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; } // اسم الفئة بدلاً من الـ ID
        public string BrandName { get; set; }    // اسم البراند بدلاً من الـ ID
        public string? UrlImage { get; set; }
        public bool IsOffer { get; set; }
        public string Discount { get; set; }     // نسبة الخصم المحسوبة (مثل 20%)



    }
    public class CreateProductDto
    {
        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [StringLength(150)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(1, 100000)]
        public decimal Price { get; set; }

        public decimal OldPrice { get; set; }

        public bool IsOffer { get; set; } = false;

        public string? UrlImage { get; set; }

        [Range(0, 1000)]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "يجب تحديد الفئة")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "يجب تحديد العلامة التجارية")]
        public int BrandId { get; set; } // 👈 مهم جداً للربط الجديد
    }
}