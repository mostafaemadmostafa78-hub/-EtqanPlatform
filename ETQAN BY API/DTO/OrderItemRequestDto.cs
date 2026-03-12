using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class OrderItemRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 100, ErrorMessage = "الكمية يجب أن تكون بين 1 و 100")]
        public int Quantity { get; set; }
    }
}
