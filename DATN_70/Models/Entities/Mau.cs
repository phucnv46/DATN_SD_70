using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class Mau
    {
        [Key]
        [MaxLength(20)]
        public string MauID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ten { get; set; }
        public ICollection<ChiTietSanPham> ChiTietSanPhams { get; set; }

        public ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; }
    }
}
