using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

public class ProductCreateRequest
{
    public string Ten { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string DanhMucID { get; set; } = string.Empty;
    public string ThuongHieuID { get; set; } = string.Empty;

    // Danh sách các biến thể (Size/Màu/Giá)
    public List<ProductVariantRequest> BienThes { get; set; } = new();

    // File ảnh thực tế gửi từ máy tính
    public IFormFile? FileAnh { get; set; }
}