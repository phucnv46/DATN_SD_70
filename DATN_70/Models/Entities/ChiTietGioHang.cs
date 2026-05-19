using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class ChiTietGioHang
    {
        [Key]
        [MaxLength(36)]
        public string ChiTietGioHangID { get; set; }

        [Required]
        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal TongTien { get; set; }

        [MaxLength(36)]
        public string GioHangID { get; set; }

        [MaxLength(36)]
        public string ChiTietSanPhamID { get; set; }

        public GioHang GioHang { get; set; }
        public ChiTietSanPham ChiTietSanPham { get; set; }
    }
}
