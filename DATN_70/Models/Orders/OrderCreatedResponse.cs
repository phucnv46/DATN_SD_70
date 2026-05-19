namespace DATN_70.Models.Orders;

public sealed class OrderCreatedResponse
{
    public string HoaDonID { get; set; } = string.Empty;

    public string TenKhachHang { get; set; } = string.Empty;

    public string SoDienThoai { get; set; } = string.Empty;

    public string DiaChiGiaoHang { get; set; } = string.Empty;

    public DateTime NgayTao { get; set; }

    public decimal TongTien { get; set; }

    public int TrangThai { get; set; }

    public List<OrderCreatedItemResponse> ChiTiet { get; set; } = [];
}
