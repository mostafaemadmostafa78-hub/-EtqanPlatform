namespace ETQAN_BY_API.DTO
{
    public class IssueInvoiceDto
    {
        public int ServiceRequestId { get; set; }
        public List<ArtisanInvoiceItemDto> Items { get; set; }
    }

    public class ArtisanInvoiceItemDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalItemPrice => Price * Quantity;
    }

    public class CompanyInvoiceItemDto
    {
        public string ItemName { get; set; } 
        public decimal UnitPrice { get; set; } 
        public int Quantity { get; set; } 
        public decimal TotalItemPrice => UnitPrice * Quantity;
    }

}
