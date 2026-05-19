namespace DATN_70.Models.Orders;

public sealed class OrderCreatedItemResponse
{
    public string HoaDonChiTietID { get; set; } = string.Empty;

    public string ChiTietSanPhamID { get; set; } = string.Empty;

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal ThanhTien { get; set; }
}
