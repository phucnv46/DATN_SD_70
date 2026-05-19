namespace DATN_70.Models.Products;

public sealed class ProductDetailResponse
{
    public string SanPhamID { get; set; } = string.Empty;

    public string Ten { get; set; } = string.Empty;

    public string MoTa { get; set; } = string.Empty;

    public List<ProductVariantResponse> BienThe { get; set; } = [];
}
