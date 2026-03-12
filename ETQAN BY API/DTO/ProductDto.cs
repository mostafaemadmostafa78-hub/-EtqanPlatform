namespace ETQAN_BY_API.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; }
        public string? UrlImage { get; set; }
        public double Rating { get; set; }
        public int Reviews { get; set; }
        public string Discount { get; set; }
        public bool IsOffer { get; set; } // ✅ عشان نفرق بين عرض ومنتج عادي
    }

    
}