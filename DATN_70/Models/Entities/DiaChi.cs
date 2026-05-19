using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class DiaChi
    {
        [Key]
        [MaxLength(36)]
        public string DiaChiID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenNguoiNhan { get; set; }

        [Required]
        [MaxLength(15)]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$")]
        public string SoDienThoaiNhan { get; set; }

        [Required]
        [MaxLength(100)]
        public string TinhThanh { get; set; }

        [Required]
        [MaxLength(100)]
        public string QuanHuyen { get; set; }

        [Required]
        [MaxLength(100)]
        public string PhuongXa { get; set; }
        public bool LaMacDinh { get; set; }        
        public string KhachHangID { get; set; }

        [MaxLength(20)]
        public KhachHang KhachHang { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }
}
