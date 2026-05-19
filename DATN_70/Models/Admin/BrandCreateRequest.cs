using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Admin
{
    public class BrandCreateRequest
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100)]
        public string Ten { get; set; }

        [MaxLength(500)]
        public string? MoTa { get; set; }

        // Thay đổi từ string thành IFormFile để nhận file từ client
        [Required(ErrorMessage = "Vui lòng chọn logo cho thương hiệu")]
        public IFormFile LogoFile { get; set; }
    }
}