using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Orders;

public sealed class OrderItemRequest
{
    [Required]
    [StringLength(20)]
    public string ChiTietSanPhamID { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int SoLuong { get; set; }
}
