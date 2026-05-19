namespace DATN_70.Data;

public sealed class StorefrontOrderItem
{
    public string HoaDonChiTietID { get; set; } = string.Empty;
    public string HoaDonID { get; set; } = string.Empty;
    public string ChiTietSanPhamID { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }

    public StorefrontOrder HoaDon { get; set; } = null!;
}
