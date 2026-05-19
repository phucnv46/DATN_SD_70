using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AspNetCoreGeneratedDocument;

namespace DATN_70.Models.Entities
{
    public class SanPham
    {
        [Key]
        [MaxLength(20)]
        public string SanPhamID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [MaxLength(200)]
        public string Ten { get; set; }

        [Range(0, 100)]
        public int MucVAT { get; set; }

        [MaxLength(100)]
        public string? ChatLieu { get; set; }
        public string MoTa { get; set; }

        [MaxLength(20)]
        public string ThuongHieuID { get; set; }

        [MaxLength(20)]
        public string DanhMucID { get; set; }

        public ThuongHieu ThuongHieu { get; set; }
        public DanhMuc DanhMuc { get; set; }

        public ICollection<ChiTietSanPham> ChiTietSanPhams { get; set; }
        public ICollection<KhuyenMaiSanPham> KhuyenMaiSanPhams { get; set; }

        public ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; }
    }
}
