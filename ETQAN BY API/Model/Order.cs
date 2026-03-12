using System.ComponentModel.DataAnnotations;

namespace ETQAN.API.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalPrice { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}