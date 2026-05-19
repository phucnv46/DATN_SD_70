using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DATN_70.Models.Enums.Enums;

namespace DATN_70.Models.Entities
{
    public class ChiTietDoiTra
    {
        [Key]
        [MaxLength(36)]
        public string ChiTietDoiTraID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(36)]
        public string PhieuDoiTraID { get; set; }

        [Required]
        [MaxLength(36)]
        public string ChiTietSanPhamID { get; set; }

        [Required]
        public int SoLuongTra { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaTriHoanLai { get; set; } // SoLuongTra * DonGia lúc mua

        public LyDoDoiTra LyDo { get; set; }

        public PhieuDoiTra PhieuDoiTra { get; set; }
        public ChiTietSanPham ChiTietSanPham { get; set; }
    }
}