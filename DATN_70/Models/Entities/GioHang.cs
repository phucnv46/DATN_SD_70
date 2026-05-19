using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class GioHang
    {
        [Key]
        [MaxLength(36)]
        public string GioHangID { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [MaxLength(36)]
        public string TaiKhoanID { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
