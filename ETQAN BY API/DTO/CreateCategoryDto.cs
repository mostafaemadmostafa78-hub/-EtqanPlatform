using System.ComponentModel.DataAnnotations;

namespace ETQAN_BY_API.DTO
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
