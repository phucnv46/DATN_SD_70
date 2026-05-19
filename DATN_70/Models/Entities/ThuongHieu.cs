using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class ThuongHieu
    {
        [Key]
        [MaxLength(20)]
        public string ThuongHieuID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; }

        [MaxLength(500)]
        public string LogoURL { get; set; }

        [MaxLength(500)]
        public string MoTa { get; set; }
        public ICollection<SanPham> SanPhams { get; set; }
    }
}
