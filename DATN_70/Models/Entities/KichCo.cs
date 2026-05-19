using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class KichCo
    {
        [Key]
        [MaxLength(20)]
        public string KichCoID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ten { get; set; }

        [MaxLength(255)]
        public string MoTa { get; set; }
        public ICollection<ChiTietSanPham> ChiTietSanPhams { get; set; }
    }
}
