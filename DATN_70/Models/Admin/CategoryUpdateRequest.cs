using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Admin
{
    public class CategoryUpdateRequest
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100)]
        public string Ten { get; set; } = string.Empty;
    }
}