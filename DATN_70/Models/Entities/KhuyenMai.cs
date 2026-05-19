using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DATN_70.Models.Enums.Enums;

namespace DATN_70.Models.Entities
{
    public class KhuyenMai
    {
        [Key]
        [MaxLength(20)]
        public string KhuyenMaiID { get; set; }

        // --- CỘT MỚI: MÃ VOUCHER ---
        [MaxLength(50)]
        public string? MaCode { get; set; } // Khách nhập mã này để giảm. Nếu để NULL thì là tự động giảm.

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; }

        // --- THAY THẾ CỘT PhanTramChietKhau THÀNH BỘ ĐÔI NÀY ---
        public LoaiGiamGia LoaiGiamGia { get; set; } // 0: Trừ thẳng tiền, 1: Phần trăm

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaTriGiam { get; set; } // Nếu LoaiGiamGia=1 thì đây là % (vd: 20). Nếu LoaiGiamGia=0 thì là tiền (vd: 50000)

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiamToiDa { get; set; } // Trần giảm giá (Dùng khi giảm %, ví dụ: Giảm 20% nhưng TỐI ĐA 100k)

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaTriToiThieuApDung { get; set; } // Đơn từ bao nhiêu tiền thì được áp dụng

        // --- BỔ SUNG CỘT GIỚI HẠN SỐ LƯỢNG ---
        public int SoLuongToiDa { get; set; } // Tổng số lượng mã phát ra
        public int SoLuongDaDung { get; set; } = 0; // Đếm số người đã xài

        public DateTime NgayApDung { get; set; }
        public DateTime NgayKetThuc { get; set; }

        [MaxLength(500)]
        public string MoTa { get; set; }

        public TrangThaiHoatDong TrangThai { get; set; }

        // --- GIỮ NGUYÊN QUAN HỆ CŨ CỦA BẠN ---
        public ICollection<HoaDon> HoaDons { get; set; }
        public ICollection<KhuyenMaiSanPham> KhuyenMaiSanPhams { get; set; } // Bảng phụ để áp dụng cho 1 hoặc nhiều sản phẩm cụ thể
    }
}