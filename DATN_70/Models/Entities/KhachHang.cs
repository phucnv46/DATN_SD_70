using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DATN_70.Models.Enums.Enums;
namespace DATN_70.Models.Entities
{
    public class KhachHang
    {
        [Key]
        [MaxLength(20)]
        public string KhachHangID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(15)]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại VN không hợp lệ")]
        public string SoDienThoai { get; set; }
        public GioiTinh GioiTinh { get; set; }

        [MaxLength(255)]
        public string DiaChi { get; set; }
        public int DiemTichLuy { get; set; } = 0;        

        [MaxLength(36)]
        public string TaiKhoanID { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }
}
