using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class OrderRequestDto
    {
        [Required]
        public string ApplicationUserId { get; set; }

        [MinLength(1, ErrorMessage = "يجب إضافة منتج واحد على الأقل للطلب")]
        public List<OrderItemRequestDto> Items { get; set; }
    }
}
