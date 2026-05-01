namespace ETQAN.API.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
        //ah
        public string? ServiceName { get; set; }
        public decimal TotalPrice { get; set; }
        public string? CustomItemName { get; set; }

        //.
    }
}