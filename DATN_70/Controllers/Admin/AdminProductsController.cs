using DATN_70.Attributes;
using DATN_70.Data;
using DATN_70.Models.Admin;
using DATN_70.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DATN_70.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[CustomAuthorize("R01", "R02")]
public sealed class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminProductsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // 1. LẤY DANH SÁCH TOÀN BỘ SẢN PHẨM HỆ THỐNG
    [HttpGet]
    public async Task<IReadOnlyList<object>> GetAll()
    {
        var result = await _context.SanPhams
            .Include(s => s.DanhMuc)
            .Include(s => s.ThuongHieu)
            .Select(s => new {
                s.SanPhamID,
                s.Ten,
                DanhMuc = s.DanhMuc.Ten,
                ThuongHieu = s.ThuongHieu.Ten,
                SoBienThe = _context.ChiTietSanPhams.Count(ct => ct.SanPhamID == s.SanPhamID)
            })
            .ToListAsync();
        return result;
    }

    // 2. XEM CHI TIẾT MỘT SẢN PHẨM KÈM THEO DANH SÁCH BIẾN THỂ
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _context.SanPhams
            .Include(s => s.ChiTietSanPhams).ThenInclude(ct => ct.Mau)
            .Include(s => s.ChiTietSanPhams).ThenInclude(ct => ct.KichCo)
            .Include(s => s.HinhAnhSanPhams)
            .FirstOrDefaultAsync(s => s.SanPhamID == id);

        if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });

        return Ok(new
        {
            product.SanPhamID,
            product.Ten,
            product.MoTa,
            product.DanhMucID,
            product.ThuongHieuID,
            product.ChatLieu,
            product.MucVAT,
            // Trả về danh sách ảnh theo màu
            HinhAnhs = product.HinhAnhSanPhams.Select(h => new {
                h.MauID,
                h.Url,
                h.IsMain
            }).ToList(),
            BienThes = product.ChiTietSanPhams.Select(ct => new {
                ct.ChiTietSanPhamID,
                ct.MauID,
                TenMau = ct.Mau.Ten,
                ct.KichCoID,
                TenKichCo = ct.KichCo.Ten,
                ct.GiaNiemYet,
                ct.SoLuongTonKho,
                ct.SKU
            })
        });
    }

    // 3. THÊM MỚI SẢN PHẨM
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var productId = "SP" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        var sanPham = new SanPham
        {
            SanPhamID = productId,
            Ten = request.Ten,
            MoTa = request.MoTa ?? string.Empty,
            DanhMucID = request.DanhMucID,
            ThuongHieuID = request.ThuongHieuID,
            MucVAT = request.MucVAT
        };
        _context.SanPhams.Add(sanPham);

        // Kiểm tra trùng biến thể trong request
        var variantSet = new HashSet<string>();
        foreach (var vt in request.BienThes)
        {
            var key = $"{vt.MauID}_{vt.KichCoID}";
            if (!variantSet.Add(key))
            {
                return BadRequest(new { message = $"Biến thể Màu={vt.MauID}, Size={vt.KichCoID} bị trùng lặp." });
            }

            var sku = $"{productId}-{vt.MauID}-{vt.KichCoID}";
            if (await _context.ChiTietSanPhams.AnyAsync(c => c.SKU == sku))
            {
                return BadRequest(new { message = $"Biến thể {sku} đã tồn tại trong hệ thống." });
            }

            var variantId = "CT" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var chiTiet = new ChiTietSanPham
            {
                ChiTietSanPhamID = variantId,
                SanPhamID = productId,
                KichCoID = vt.KichCoID,
                MauID = vt.MauID,
                GiaNiemYet = vt.GiaNiemYet,
                SoLuongTonKho = vt.SoLuongTonKho,
                SKU = sku
            };
            _context.ChiTietSanPhams.Add(chiTiet);
        }

        // Xử lý ảnh theo từng màu
        if (request.ColorImages != null && request.ColorImages.Any())
        {
            var wwwRootPath = _env.WebRootPath;
            var uploadedMauIDs = new HashSet<string>();

            foreach (var colorImg in request.ColorImages)
            {
                if (colorImg.FileAnh == null || string.IsNullOrWhiteSpace(colorImg.MauID))
                    continue;

                // Mỗi màu chỉ lấy 1 ảnh
                if (!uploadedMauIDs.Add(colorImg.MauID))
                    continue;

                var fileName = Guid.NewGuid().ToString() + "_" + colorImg.FileAnh.FileName;
                var path = Path.Combine(wwwRootPath, "images/products", fileName);
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await colorImg.FileAnh.CopyToAsync(fileStream);
                }

                var hinhAnh = new HinhAnhSanPham
                {
                    HinhAnhID = Guid.NewGuid().ToString(),
                    SanPhamID = productId,
                    MauID = colorImg.MauID,
                    Url = "/images/products/" + fileName,
                    IsMain = true
                };
                _context.HinhAnhSanPhams.Add(hinhAnh);
            }
        }
        // Nếu không có ảnh màu, có thể vẫn cho phép tạo sản phẩm không ảnh

        await _context.SaveChangesAsync();
        return Ok(new { message = "Tạo sản phẩm và biến thể thành công!", productId });
    }

    // 4. CẬP NHẬT THÔNG TIN VÀ BIẾN THỂ KHO HÀNG (UPDATE)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromForm] ProductUpdateRequest request)
    {
        var sanPham = await _context.SanPhams
            .Include(s => s.ChiTietSanPhams)
            .Include(s => s.HinhAnhSanPhams)
            .FirstOrDefaultAsync(s => s.SanPhamID == id);

        if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });

        sanPham.Ten = request.Ten;
        sanPham.MoTa = request.MoTa ?? string.Empty;
        sanPham.DanhMucID = request.DanhMucID;
        sanPham.ThuongHieuID = request.ThuongHieuID;
        sanPham.MucVAT = request.MucVAT;

        // Xử lý ảnh theo màu nếu có
        if (request.ColorImages != null && request.ColorImages.Any())
        {
            var wwwRootPath = _env.WebRootPath;
            foreach (var colorImg in request.ColorImages)
            {
                if (colorImg.FileAnh == null || string.IsNullOrWhiteSpace(colorImg.MauID))
                    continue;

                // Xóa ảnh cũ của màu này
                var oldImages = _context.HinhAnhSanPhams
                    .Where(h => h.SanPhamID == id && h.MauID == colorImg.MauID);
                _context.HinhAnhSanPhams.RemoveRange(oldImages);

                // Lưu ảnh mới
                var fileName = Guid.NewGuid().ToString() + "_" + colorImg.FileAnh.FileName;
                var path = Path.Combine(wwwRootPath, "images/products", fileName);
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await colorImg.FileAnh.CopyToAsync(fileStream);
                }

                var hinhAnh = new HinhAnhSanPham
                {
                    HinhAnhID = Guid.NewGuid().ToString(),
                    SanPhamID = id,
                    MauID = colorImg.MauID,
                    Url = "/images/products/" + fileName,
                    IsMain = true
                };
                _context.HinhAnhSanPhams.Add(hinhAnh);
            }
        }

        // Đồng bộ danh sách biến thể
        if (request.BienThes != null)
        {
            // Kiểm tra trùng trong danh sách gửi lên
            var sentVariantKeys = new HashSet<string>();
            foreach (var vt in request.BienThes)
            {
                var key = $"{vt.MauID}_{vt.KichCoID}";
                if (!sentVariantKeys.Add(key))
                {
                    return BadRequest(new { message = $"Biến thể Màu={vt.MauID}, Size={vt.KichCoID} bị trùng lặp." });
                }
            }

            foreach (var vt in request.BienThes)
            {
                var existingVariant = sanPham.ChiTietSanPhams
                    .FirstOrDefault(ct => ct.MauID == vt.MauID && ct.KichCoID == vt.KichCoID);

                if (existingVariant != null)
                {
                    existingVariant.GiaNiemYet = vt.GiaNiemYet;
                    existingVariant.SoLuongTonKho = vt.SoLuongTonKho;
                }
                else
                {
                    var variantId = "CT" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                    var chiTiet = new ChiTietSanPham
                    {
                        ChiTietSanPhamID = variantId,
                        SanPhamID = id,
                        KichCoID = vt.KichCoID,
                        MauID = vt.MauID,
                        GiaNiemYet = vt.GiaNiemYet,
                        SoLuongTonKho = vt.SoLuongTonKho,
                        SKU = $"{id}-{vt.MauID}-{vt.KichCoID}"
                    };
                    _context.ChiTietSanPhams.Add(chiTiet);
                }
            }

            // Dòng nào biến mất trên giao diện thì xóa khỏi hệ thống (nếu đơn hàng chưa sử dụng)
            foreach (var dbVt in sanPham.ChiTietSanPhams.ToList())
            {
                if (!request.BienThes.Any(vt => vt.MauID == dbVt.MauID && vt.KichCoID == dbVt.KichCoID))
                {
                    var hasOrder = await _context.HoaDonChiTiets.AnyAsync(h => h.ChiTietSanPhamID == dbVt.ChiTietSanPhamID);
                    if (!hasOrder)
                    {
                        _context.ChiTietSanPhams.Remove(dbVt);
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật sản phẩm thành công!" });
    }

    // 5. XÓA VĨNH VIỄN SẢN PHẨM KHỎI HỆ THỐNG (DELETE)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var sanPham = await _context.SanPhams
            .Include(s => s.ChiTietSanPhams)
            .Include(s => s.HinhAnhSanPhams)
            .FirstOrDefaultAsync(s => s.SanPhamID == id);

        if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });

        foreach (var vt in sanPham.ChiTietSanPhams)
        {
            var hasOrders = await _context.HoaDonChiTiets.AnyAsync(hd => hd.ChiTietSanPhamID == vt.ChiTietSanPhamID);
            if (hasOrders)
            {
                return BadRequest(new { message = "Không thể xóa sản phẩm này vì đã phát sinh hóa đơn mua hàng thực tế!" });
            }
        }

        _context.HinhAnhSanPhams.RemoveRange(sanPham.HinhAnhSanPhams);
        _context.ChiTietSanPhams.RemoveRange(sanPham.ChiTietSanPhams);
        _context.SanPhams.Remove(sanPham);

        await _context.SaveChangesAsync();
        return Ok(new { message = "Xóa sản phẩm thành công khỏi hệ thống!" });
    }
}

#region Data Transfer Objects (DTOs)
public class ProductUpdateRequest
{
    [Required]
    public string Ten { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    [Required]
    public string DanhMucID { get; set; } = string.Empty;
    [Required]
    public string ThuongHieuID { get; set; } = string.Empty;
    public IFormFile? FileAnh { get; set; }
    public List<VariantUpdateRequest> BienThes { get; set; } = new();
    public int MucVAT { get; set; }
    public List<ColorImageRequest>? ColorImages { get; set; }
}

public class VariantUpdateRequest
{
    [Required]
    public string MauID { get; set; } = string.Empty;
    [Required]
    public string KichCoID { get; set; } = string.Empty;
    [Required]
    public decimal GiaNiemYet { get; set; }
    [Required]
    public int SoLuongTonKho { get; set; }
}
#endregion