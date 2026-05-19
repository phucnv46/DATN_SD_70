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
<<<<<<< HEAD

        public ICollection<HinhAnhSanPham> HinhAnhSanPhams { get; set; }
=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
    }
}
