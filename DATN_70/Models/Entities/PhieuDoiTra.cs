using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DATN_70.Models.Enums.Enums;

namespace DATN_70.Models.Entities
{
    public class PhieuDoiTra
    {
        [Key]
        [MaxLength(36)]
        public string PhieuDoiTraID { get; set; } = "RMA" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        [Required]
        [MaxLength(36)]
        public string HoaDonID { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public TrangThaiDoiTra TrangThai { get; set; } = TrangThaiDoiTra.ChoXuLy;

        [Column(TypeName = "decimal(18,0)")]
        public decimal TongTienHoan { get; set; }

        [MaxLength(500)]
        public string? GhiChuAdmin { get; set; }

        public HoaDon HoaDon { get; set; }
        public ICollection<ChiTietDoiTra> ChiTietDoiTras { get; set; }
    }
}