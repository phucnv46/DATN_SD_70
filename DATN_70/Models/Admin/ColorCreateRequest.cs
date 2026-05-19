using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Admin
{
    public class ColorCreateRequest
    {
        [Required(ErrorMessage = "Tên màu không được để trống")]
        [MaxLength(50)]
        public string Ten { get; set; } = string.Empty;
    }
}