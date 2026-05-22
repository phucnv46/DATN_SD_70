using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Orders;

public sealed class PlaceOrderRequest
{
    [Required(ErrorMessage = "Mã khách hàng không hợp lệ.")]
    public string KhachHangID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã địa chỉ không hợp lệ.")]
    public string DiaChiID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên người nhận không được để trống.")]
    public string TenKhachHang { get; set; } = string.Empty;

    [Required]
    [StringLength(15)]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string DiaChiGiaoHang { get; set; } = string.Empty;

    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = [];
    public string? PaymentMethod { get; set; }
    public decimal ShippingFee { get; set; }
}