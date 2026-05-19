using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class TaiKhoan
    {
        [Key]
        [MaxLength(36)]
        public string TaiKhoanID { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string MatKhau { get; set; }

        [MaxLength(50)]
        public string TrangThai { get; set; }

        [MaxLength(20)]
        public string VaiTroID { get; set; }
        public VaiTro VaiTro { get; set; }
        public GioHang GioHang { get; set; }        
        public KhachHang KhachHang { get; set; }
        public NhanVien NhanVien { get; set; }
    }
}
