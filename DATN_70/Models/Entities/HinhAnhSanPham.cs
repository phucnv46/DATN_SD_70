using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_70.Models.Entities
{
    public class HinhAnhSanPham
    {
        [Key]
        [MaxLength(36)]
        public string HinhAnhID { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Đường dẫn ảnh không được để trống")]
        [MaxLength(500)]
        public string Url { get; set; }

        public bool IsMain { get; set; } // true nếu là ảnh đại diện của màu đó

        [MaxLength(20)]
        public string SanPhamID { get; set; }

        [MaxLength(20)]
        public string? MauID { get; set; } // Cho phép null (ảnh chung chung), hoặc gắn ID màu (ảnh theo màu)

        // Navigation properties
        [ForeignKey("SanPhamID")]
        public SanPham SanPham { get; set; }

        [ForeignKey("MauID")]
        public Mau Mau { get; set; }
    }
}