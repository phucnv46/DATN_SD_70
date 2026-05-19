using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class KhuyenMaiSanPham
    {
        [MaxLength(20)]
        public string KhuyenMaiID { get; set; }
        public KhuyenMai KhuyenMai { get; set; }

        [MaxLength(20)]
        public string SanPhamID { get; set; }
        public SanPham SanPham { get; set; }
    }
}
