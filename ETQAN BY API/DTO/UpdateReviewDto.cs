//ah
using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.Model.DTOs
{
    public class UpdateReviewDto
    {
        [Range(1, 5)]
        public int Rating { get; set; } // عدد النجوم من 1 لـ 5

        [StringLength(500)]
        public string Comment { get; set; } // رأي العميل الجديد
    }
}

//.