using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DATN_70.Models.Entities
{
    public class VaiTro
    {
        [Key]
        [MaxLength(20)]
        public string VaiTroID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; }
        public ICollection<TaiKhoan> TaiKhoans { get; set; }
    }
}
