namespace DATN_70.Models.Products;

public sealed class ProductListItemResponse
{
    public string SanPhamID { get; set; } = string.Empty;

    public string Ten { get; set; } = string.Empty;

    public string MoTa { get; set; } = string.Empty;

    public decimal GiaThapNhat { get; set; }

    public decimal GiaGoc { get; set; }

    public decimal PhanTramGiam { get; set; }

    public int TongSoLuongTon { get; set; }
<<<<<<< HEAD
    public string HinhAnhDaiDien { get; set; } = string.Empty;
    public int LoaiGiamGia { get; internal set; }
    public decimal GiaTriGiam { get; internal set; }
    public decimal GiamToiDa { get; internal set; }
=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
}
