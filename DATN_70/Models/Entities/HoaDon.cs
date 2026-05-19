using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DATN_70.Models.Enums.Enums;
namespace DATN_70.Models.Entities
{
    public class HoaDon
    {
        [Key]
        [MaxLength(36)]
        public string HoaDonID { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public double TongTienVAT { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public double TongTienGiamGia { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public double ThanhTien { get; set; }
        public LoaiGiaoDich LoaiGiaoDich { get; set; }   // 0: Online, 1: POS Tại quầy
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public TrangThaiHoaDon TrangThai { get; set; } // 0: Chờ xác nhận, 1: Đã xác nhận 2: Đang chuẩn bị 3: Đang giao, 4: Hoàn thành, 5: Đã hủy

        [MaxLength(500)]
        public string GhiChu { get; set; }

        [MaxLength(20)]
        public string KhachHangID { get; set; }

        [MaxLength(20)]
        public string NhanVienID { get; set; }

        [MaxLength(36)]
        public string DiaChiID { get; set; }

        [MaxLength(20)]
        public string? KhuyenMaiID { get; set; }  
        
        public KhachHang KhachHang { get; set; }
        public NhanVien NhanVien { get; set; }
        public DiaChi DiaChi { get; set; }
        public KhuyenMai KhuyenMai { get; set; }
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; }
        public ICollection<ChiTietThanhToan> ChiTietThanhToans { get; set; }
    }
}
