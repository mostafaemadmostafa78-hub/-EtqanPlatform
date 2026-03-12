namespace ETQAN_BY_API.DTO
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? UrlImage { get; set; }
        public bool IsOffer { get; set; }
    }
}