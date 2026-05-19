using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Admin
{
    public class BrandUpdateRequest
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100)]
        public string Ten { get; set; }

        [MaxLength(500)]
        public string? MoTa { get; set; }

        // Khi sửa có thể chọn file mới hoặc giữ file cũ (nên không để [Required])
        public IFormFile? LogoFile { get; set; }
    }
}
