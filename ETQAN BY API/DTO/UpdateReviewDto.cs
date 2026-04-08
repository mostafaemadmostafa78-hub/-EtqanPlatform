//ah
using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.Model.DTOs
{
    public class UpdateReviewDto
    {
        [Required(ErrorMessage = "معرف الحرفي مطلوب")]
        public int ArtisanId { get; set; } // ضروري عشان نعرف التقييم رايح لمين

        [Required(ErrorMessage = "برجاء تحديد عدد النجوم")]
        [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5 نجوم")]
        public int Rating { get; set; } 

        [Required(ErrorMessage = "برجاء كتابة رأيك")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "التعليق يجب أن يكون بين 3 و 500 حرف")]
        public string Comment { get; set; }
    }

}    

//.