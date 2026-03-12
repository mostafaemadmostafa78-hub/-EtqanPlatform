namespace ETQAN_BY_API.DTO
{
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemResponseDto> Details { get; set; }
    }
}
