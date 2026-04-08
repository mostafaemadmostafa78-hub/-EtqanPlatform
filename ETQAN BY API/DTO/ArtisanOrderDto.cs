//ah
namespace ETQAN_BY_API.Model.DTOs
{
    public class ArtisanOrderDto
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; }
        public string Location { get; set; } // المنصورة - المشاية مثلاً
        public string OrderDate { get; set; }
        public string ServiceName { get; set; } // صيانة أسلاك وتوصيلات
        public string Status { get; set; } // قيد الانتظار، مقبول، إلخ
    }
}
//.