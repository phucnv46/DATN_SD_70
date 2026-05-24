using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ProductCreateRequest
{
    [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
    public string Ten { get; set; } = string.Empty;

    public string? MoTa { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
    public string DanhMucID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn thương hiệu.")]
    public string ThuongHieuID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phải có ít nhất một biến thể.")]
    [MinLength(1, ErrorMessage = "Phải có ít nhất một biến thể.")]
    public List<ProductVariantRequest> BienThes { get; set; } = new();

    public IFormFile? FileAnh { get; set; }
    public int MucVAT { get; set; } = 10;
    public List<ColorImageRequest>? ColorImages { get; set; }
}