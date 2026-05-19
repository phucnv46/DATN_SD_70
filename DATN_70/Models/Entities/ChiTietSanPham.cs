using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class ChiTietSanPham
    {
        [Key]
        [MaxLength(36)]
        public string ChiTietSanPhamID { get; set; }

        [Required]
        public int SoLuongTonKho { get; set; }

        [Required]
        [MaxLength(50)]
        public string SKU { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaNiemYet { get; set; }

        [MaxLength(20)]
        public string KichCoID { get; set; }

        [MaxLength(20)]
        public string MauID { get; set; }

        [MaxLength(20)]
        public string SanPhamID { get; set; }
        
        public KichCo KichCo { get; set; }        
        public Mau Mau { get; set; }
        public SanPham SanPham { get; set; }
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; }
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
