using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DATN_70.Models.Enums.Enums;
namespace DATN_70.Models.Entities
{
    public class PhuongThucThanhToan
    {
        [Key]
        [MaxLength(20)]
        public string PhuongThucThanhToanID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; }        
        public KieuThanhToan KieuThanhToan { get; set; }

        [MaxLength(500)]
        public string HinhURL { get; set; }
        public TrangThaiHoatDong TrangThai { get; set; }   
        public ICollection<ChiTietThanhToan> ChiTietThanhToans { get; set; }
    }
}
