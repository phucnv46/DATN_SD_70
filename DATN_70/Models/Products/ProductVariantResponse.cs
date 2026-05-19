namespace DATN_70.Models.Products;

public sealed class ProductVariantResponse
{
    public string ChiTietSanPhamID { get; set; } = string.Empty;

    public string KichCoID { get; set; } = string.Empty;

    public string TenKichCo { get; set; } = string.Empty;

    public string MauID { get; set; } = string.Empty;

    public string TenMau { get; set; } = string.Empty;

    public decimal GiaNiemYet { get; set; }

    public decimal GiaGoc { get; set; }

    public decimal PhanTramGiam { get; set; }

    public int SoLuongTon { get; set; }
    public string HinhAnhUrl { get; set; } = string.Empty;
    public int LoaiGiamGia { get; internal set; }
    public decimal GiaTriGiam { get; internal set; }
    public decimal GiamToiDa { get; internal set; }
}
