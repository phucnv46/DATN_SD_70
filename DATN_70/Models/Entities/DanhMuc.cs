using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class DanhMuc
    {
        [Key]
        [MaxLength(20)]
        public string DanhMucID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; }
        public ICollection<SanPham> SanPhams { get; set; }
    }
}
