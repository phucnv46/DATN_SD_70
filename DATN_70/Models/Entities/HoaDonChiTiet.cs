using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class HoaDonChiTiet
    {
        [Key]
        [MaxLength(36)]
        public string HoaDonChiTietID { get; set; }

        [Required]
        public int SoLuong { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        public decimal DonGia { get; set; }
        public decimal MucVAT { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal TienVAT { get; set; }

        [MaxLength(36)]
        public string HoaDonID { get; set; }

        [MaxLength(36)]
        public string ChiTietSanPhamID { get; set; }

        public HoaDon HoaDon { get; set; }
        public ChiTietSanPham ChiTietSanPham { get; set; }
    }
}
