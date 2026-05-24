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

        [MaxLength(50)]
        public string? MaCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; }

        public LoaiGiamGia LoaiGiamGia { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaTriGiam { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiamToiDa { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaTriToiThieuApDung { get; set; }

        public int SoLuongToiDa { get; set; }
        public int SoLuongDaDung { get; set; } = 0;

        public DateTime NgayApDung { get; set; }
        public DateTime NgayKetThuc { get; set; }

        [MaxLength(500)]
        public string MoTa { get; set; }

        public TrangThaiHoatDong TrangThai { get; set; }

        [MaxLength(20)]
        public string? DanhMucID { get; set; }

        [ForeignKey("DanhMucID")]
        public DanhMuc? DanhMucRef { get; set; } // Tên tránh trùng với class DanhMuc

        public ICollection<HoaDon> HoaDons { get; set; }
        public ICollection<KhuyenMaiSanPham> KhuyenMaiSanPhams { get; set; }
    }
}