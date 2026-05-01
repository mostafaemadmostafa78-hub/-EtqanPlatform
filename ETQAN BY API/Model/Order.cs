using ETQAN.API.Models.Enums;
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
        //ah
        public bool IsServiceOrder { get; set; } // تمييز النوع :خدمة/متجر
        public string? ArtisanId { get; set; } // الحرفي المسؤول
        public Artisan? Artisan { get; set; } // علاقة لجلب بيانات الحرفي

        public int? ServiceRequestId { get; set; } // رابط للطلب الأصلي عشان م نكررش الوصف
        public ServiceRequest? ServiceRequest { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        //.
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}