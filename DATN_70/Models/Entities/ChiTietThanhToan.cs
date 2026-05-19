using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DATN_70.Models.Enums.Enums;
namespace DATN_70.Models.Entities
{
    public class ChiTietThanhToan
    {
        [Key]
        [MaxLength(36)]
        public string ChiTietThanhToanID { get; set; }

        [MaxLength(100)]
        public string Ten { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal SoTien { get; set; }

        [MaxLength(100)]
        public string MaThamChieu { get; set; } // Mã giao dịch từ Ngân hàng/Ví điện tử
        public DateTime ThoiGianThanhToan { get; set; } = DateTime.Now;
        public TrangThaiThanhToan TrangThai { get; set; }

        [MaxLength(36)]
        public string HoaDonID { get; set; }

        [MaxLength(20)]
        public HoaDon HoaDon { get; set; }       
        public string PhuongThucThanhToanID { get; set; }
        public PhuongThucThanhToan PhuongThucThanhToan { get; set; }
<<<<<<< HEAD

=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
    }
}
