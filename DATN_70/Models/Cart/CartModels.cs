using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.Cart;

public sealed class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = [];
}

public sealed class CartItemResponse
{
    public string SanPhamID { get; set; } = string.Empty;
    public string ChiTietSanPhamID { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string PhanLoai { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public int TonKho { get; set; }
}

public sealed class AddCartItemRequest
{
    [Required]
    public string ChiTietSanPhamID { get; set; } = string.Empty;

    [Range(1, 20)]
    public int SoLuong { get; set; }
}

public sealed class UpdateCartItemRequest
{
    [Range(0, 20)]
    public int SoLuong { get; set; }
}
