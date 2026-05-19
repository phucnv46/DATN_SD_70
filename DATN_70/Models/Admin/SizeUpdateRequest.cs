using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Admin
{
    public class SizeUpdateRequest
    {
        [Required(ErrorMessage = "Tên kích cỡ không được để trống")]
        [MaxLength(50)]
        public string Ten { get; set; } = string.Empty;
    }
}