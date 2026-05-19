using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Entities;

public class Banner
{
    [Key]
    [MaxLength(36)]
    public string BannerID { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string TieuDe { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? MoTa { get; set; }

    [Required]
    [MaxLength(500)]
    public string HinhAnhUrl { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? LienKetUrl { get; set; }

    public int ThuTu { get; set; }

    public bool KichHoat { get; set; }
}
