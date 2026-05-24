using DATN_70.Attributes;
using DATN_70.Data;
using DATN_70.Models.Entities;
using DATN_70.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DATN_70.Controllers.Admin;

[ApiController]
[Route("api/admin/promotions")]
[CustomAuthorize("R01", "R02")]
public sealed class AdminPromotionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminPromotionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ==========================================
    // PHƯƠNG THỨC DÙNG CHUNG
    // ==========================================

    private async Task<bool> HasActiveGlobalPromo(string? excludePromoId = null)
    {
        var query = _dbContext.KhuyenMais.Where(k =>
            k.MaCode == null &&
            k.TrangThai == Enums.TrangThaiHoatDong.HoatDong &&
            !k.KhuyenMaiSanPhams.Any()
        );

        if (!string.IsNullOrEmpty(excludePromoId))
            query = query.Where(k => k.KhuyenMaiID != excludePromoId);

        return await query.AnyAsync();
    }

    private async Task<bool> IsMaCodeDuplicate(string maCode, string? excludePromoId = null)
    {
        if (string.IsNullOrWhiteSpace(maCode))
            return false;

        var query = _dbContext.KhuyenMais.Where(k => k.MaCode == maCode);
        if (!string.IsNullOrEmpty(excludePromoId))
            query = query.Where(k => k.KhuyenMaiID != excludePromoId);

        return await query.AnyAsync();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPromotions([FromQuery] string? search, [FromQuery] string? status, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.KhuyenMais.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(k => k.Ten.ToLower().Contains(term) || (k.MaCode != null && k.MaCode.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Enums.TrangThaiHoatDong>(status, true, out var parsedStatus))
            {
                query = query.Where(k => k.TrangThai == parsedStatus);
            }

            var promotions = await query
                .OrderByDescending(k => k.NgayApDung)
                .Select(k => new AdminPromoSummaryResponse
                {
                    KhuyenMaiId = k.KhuyenMaiID,
                    Ten = k.Ten,
                    MaCode = k.MaCode ?? "Tự động áp dụng",
                    LoaiGiamGia = (int)k.LoaiGiamGia == 1 ? "Phần trăm (%)" : "Trừ tiền mặt",
                    GiaTriGiam = k.GiaTriGiam,
                    NgayApDung = k.NgayApDung,
                    NgayKetThuc = k.NgayKetThuc,
                    SoLuongDaDung = k.SoLuongDaDung,
                    SoLuongToiDa = k.SoLuongToiDa,
                    TrangThai = k.TrangThai.ToString(),
                    TrangThaiInt = (int)k.TrangThai
                })
                .ToListAsync(cancellationToken);

            return Ok(promotions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tải danh sách khuyến mãi.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPromotionDetails(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var promo = await _dbContext.KhuyenMais
                .Include(k => k.KhuyenMaiSanPhams)
                    .ThenInclude(ks => ks.SanPham)
                .Include(k => k.DanhMucRef) // MỚI: lấy thông tin danh mục
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.KhuyenMaiID == id, cancellationToken);

            if (promo is null)
                return NotFound(new { message = "Không tìm thấy chương trình khuyến mãi." });

            var response = new AdminPromoDetailResponse
            {
                KhuyenMaiId = promo.KhuyenMaiID,
                Ten = promo.Ten,
                MaCode = promo.MaCode,
                MoTa = promo.MoTa ?? string.Empty,
                LoaiGiamGia = (int)promo.LoaiGiamGia,
                GiaTriGiam = promo.GiaTriGiam,
                GiamToiDa = promo.GiamToiDa,
                GiaTriToiThieuApDung = promo.GiaTriToiThieuApDung,
                SoLuongToiDa = promo.SoLuongToiDa,
                SoLuongDaDung = promo.SoLuongDaDung,
                NgayApDung = promo.NgayApDung,
                NgayKetThuc = promo.NgayKetThuc,
                TrangThai = (int)promo.TrangThai,
                DanhMucId = promo.DanhMucID,                        // MỚI
                TenDanhMuc = promo.DanhMucRef?.Ten ?? string.Empty,  // MỚI
                DanhSachSanPhamApDung = promo.KhuyenMaiSanPhams.Select(ks => new AdminPromoProductDto
                {
                    SanPhamId = ks.SanPhamID,
                    TenSanPham = ks.SanPham?.Ten ?? "N/A"
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xem chi tiết khuyến mãi.", error = ex.Message });
        }
    }

    [HttpGet("resources")]
    public async Task<IActionResult> GetPromotionResources(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _dbContext.DanhMucs.AsNoTracking()
                .Select(c => new { c.DanhMucID, c.Ten })
                .ToListAsync(cancellationToken);

            var products = await _dbContext.SanPhams.AsNoTracking()
                .Select(p => new { p.SanPhamID, p.Ten })
                .ToListAsync(cancellationToken);

            return Ok(new { categories, products });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tải tài nguyên khuyến mãi.", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePromotion([FromBody] AdminPromoCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.NgayKetThuc <= request.NgayApDung)
                return BadRequest(new { message = "Ngày kết thúc phải lớn hơn ngày áp dụng." });

            if (request.LoaiGiamGia == 1 && request.GiaTriGiam > 100)
                return BadRequest(new { message = "Mức giảm phần trăm không được vượt quá 100%." });

            // Kiểm tra trùng MaCode
            if (!string.IsNullOrWhiteSpace(request.MaCode))
            {
                if (await IsMaCodeDuplicate(request.MaCode.Trim().ToUpper()))
                    return BadRequest(new { message = "Mã Code này đã tồn tại. Vui lòng chọn mã khác." });
            }

            // Kiểm tra chỉ 1 mã toàn sàn hoạt động
            if (request.PhamVi == "all" && string.IsNullOrWhiteSpace(request.MaCode))
            {
                if (await HasActiveGlobalPromo())
                    return BadRequest(new { message = "Đã có một chương trình khuyến mãi toàn sàn đang hoạt động. Vui lòng tắt chương trình cũ trước khi tạo mới." });
            }

            var newId = $"KM{DateTime.Now.Ticks.ToString().Substring(8, 6)}";

            var promo = new KhuyenMai
            {
                KhuyenMaiID = newId,
                Ten = request.Ten.Trim(),
                MaCode = string.IsNullOrWhiteSpace(request.MaCode) ? null : request.MaCode.Trim().ToUpper(),
                MoTa = request.MoTa?.Trim() ?? string.Empty,
                LoaiGiamGia = (Enums.LoaiGiamGia)request.LoaiGiamGia,
                GiaTriGiam = request.GiaTriGiam,
                GiamToiDa = request.GiamToiDa,
                GiaTriToiThieuApDung = request.GiaTriToiThieuApDung,
                SoLuongToiDa = request.SoLuongToiDa,
                SoLuongDaDung = 0,
                NgayApDung = request.NgayApDung,
                NgayKetThuc = request.NgayKetThuc,
                TrangThai = Enums.TrangThaiHoatDong.HoatDong
            };

            // Xử lý phạm vi
            if (request.PhamVi == "category" && !string.IsNullOrWhiteSpace(request.DanhMucId))
            {
                promo.DanhMucID = request.DanhMucId; // Gán trực tiếp danh mục
            }
            else if (request.PhamVi == "product" && request.SanPhamIds != null && request.SanPhamIds.Any())
            {
                foreach (var spId in request.SanPhamIds)
                {
                    _dbContext.KhuyenMaiSanPhams.Add(new KhuyenMaiSanPham { KhuyenMaiID = newId, SanPhamID = spId });
                }
            }
            // Nếu "all" thì không cần thêm gì

            _dbContext.KhuyenMais.Add(promo);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "Tạo mã khuyến mãi thành công.", khuyenMaiId = newId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo khuyến mãi.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePromotion(string id, [FromBody] AdminPromoCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var promo = await _dbContext.KhuyenMais
                .Include(k => k.KhuyenMaiSanPhams)
                .FirstOrDefaultAsync(k => k.KhuyenMaiID == id, cancellationToken);

            if (promo is null)
                return NotFound(new { message = "Không tìm thấy chương trình khuyến mãi." });

            if (request.NgayKetThuc <= request.NgayApDung)
                return BadRequest(new { message = "Ngày kết thúc phải lớn hơn ngày áp dụng." });

            if (request.LoaiGiamGia == 1 && request.GiaTriGiam > 100)
                return BadRequest(new { message = "Mức giảm phần trăm không được vượt quá 100%." });

            string? newMaCode = string.IsNullOrWhiteSpace(request.MaCode) ? null : request.MaCode.Trim().ToUpper();

            // Kiểm tra trùng MaCode (loại trừ chính nó)
            if (!string.IsNullOrWhiteSpace(newMaCode))
            {
                if (await IsMaCodeDuplicate(newMaCode, excludePromoId: id))
                    return BadRequest(new { message = "Mã Code này đã tồn tại ở chương trình khác." });
            }

            // Nếu là toàn sàn và đang bật, kiểm tra giới hạn
            if (request.PhamVi == "all" && string.IsNullOrWhiteSpace(newMaCode) && promo.TrangThai == Enums.TrangThaiHoatDong.HoatDong)
            {
                if (await HasActiveGlobalPromo(excludePromoId: id))
                    return BadRequest(new { message = "Đã có chương trình toàn sàn khác đang hoạt động." });
            }

            // Cập nhật thông tin
            promo.Ten = request.Ten.Trim();
            promo.MaCode = newMaCode;
            promo.MoTa = request.MoTa?.Trim() ?? string.Empty;
            promo.LoaiGiamGia = (Enums.LoaiGiamGia)request.LoaiGiamGia;
            promo.GiaTriGiam = request.GiaTriGiam;
            promo.GiamToiDa = request.GiamToiDa;
            promo.GiaTriToiThieuApDung = request.GiaTriToiThieuApDung;
            promo.SoLuongToiDa = request.SoLuongToiDa;
            promo.NgayApDung = request.NgayApDung;
            promo.NgayKetThuc = request.NgayKetThuc;

            // Cập nhật phạm vi
            // Xóa hết liên kết cũ
            _dbContext.KhuyenMaiSanPhams.RemoveRange(promo.KhuyenMaiSanPhams);
            promo.DanhMucID = null;

            if (request.PhamVi == "category" && !string.IsNullOrWhiteSpace(request.DanhMucId))
            {
                promo.DanhMucID = request.DanhMucId;
            }
            else if (request.PhamVi == "product" && request.SanPhamIds != null && request.SanPhamIds.Any())
            {
                foreach (var spId in request.SanPhamIds)
                {
                    _dbContext.KhuyenMaiSanPhams.Add(new KhuyenMaiSanPham { KhuyenMaiID = id, SanPhamID = spId });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "Cập nhật chương trình thành công." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật khuyến mãi.", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePromotion(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var promo = await _dbContext.KhuyenMais
                .Include(k => k.HoaDons)
                .FirstOrDefaultAsync(k => k.KhuyenMaiID == id, cancellationToken);

            if (promo is null)
            {
                return NotFound(new { message = "Không tìm thấy chương trình khuyến mãi." });
            }

            if (promo.HoaDons.Any() || promo.SoLuongDaDung > 0)
            {
                return BadRequest(new { message = "Không thể xóa mã khuyến mãi này vì đã có khách hàng sử dụng. Bạn chỉ có thể Tắt trạng thái hoạt động." });
            }

            var now = DateTime.Now;
            if (promo.NgayApDung <= now && promo.NgayKetThuc >= now)
            {
                return BadRequest(new { message = "Không thể xóa mã khuyến mãi đang trong thời gian hiệu lực. Vui lòng chờ hết hạn hoặc tắt trạng thái." });
            }

            var linkedProducts = await _dbContext.KhuyenMaiSanPhams.Where(x => x.KhuyenMaiID == id).ToListAsync(cancellationToken);
            if (linkedProducts.Any())
            {
                _dbContext.KhuyenMaiSanPhams.RemoveRange(linkedProducts);
            }

            _dbContext.KhuyenMais.Remove(promo);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Đã xóa chương trình khuyến mãi thành công." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa khuyến mãi.", error = ex.Message });
        }
    }

    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var promo = await _dbContext.KhuyenMais
                .Include(k => k.KhuyenMaiSanPhams)
                .FirstOrDefaultAsync(k => k.KhuyenMaiID == id, cancellationToken);

            if (promo is null)
                return NotFound(new { message = "Không tìm thấy chương trình khuyến mãi." });

            if (promo.TrangThai == Enums.TrangThaiHoatDong.NgungHoatDong
                && promo.MaCode == null
                && !promo.KhuyenMaiSanPhams.Any())
            {
                if (await HasActiveGlobalPromo(excludePromoId: id))
                {
                    return BadRequest(new { message = "Không thể kích hoạt. Hiện đã có một chương trình khuyến mãi toàn sàn khác đang hoạt động." });
                }
            }

            promo.TrangThai = promo.TrangThai == Enums.TrangThaiHoatDong.HoatDong
                ? Enums.TrangThaiHoatDong.NgungHoatDong
                : Enums.TrangThaiHoatDong.HoatDong;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Đổi trạng thái thành công.",
                currentStatus = promo.TrangThai.ToString(),
                currentStatusInt = (int)promo.TrangThai
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi đổi trạng thái khuyến mãi.", error = ex.Message });
        }
    }
}

#region DTOs
public sealed class AdminPromoSummaryResponse
{
    public string KhuyenMaiId { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string MaCode { get; set; } = string.Empty;
    public string LoaiGiamGia { get; set; } = string.Empty;
    public decimal GiaTriGiam { get; set; }
    public DateTime NgayApDung { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public int SoLuongToiDa { get; set; }
    public int SoLuongDaDung { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int TrangThaiInt { get; set; }
}

public sealed class AdminPromoDetailResponse
{
    public string KhuyenMaiId { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string? MaCode { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public int LoaiGiamGia { get; set; }
    public decimal GiaTriGiam { get; set; }
    public decimal GiamToiDa { get; set; }
    public decimal GiaTriToiThieuApDung { get; set; }
    public int SoLuongToiDa { get; set; }
    public int SoLuongDaDung { get; set; }
    public DateTime NgayApDung { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public int TrangThai { get; set; }
    public string? DanhMucId { get; set; }         // MỚI
    public string TenDanhMuc { get; set; } = "";   // MỚI
    public List<AdminPromoProductDto> DanhSachSanPhamApDung { get; set; } = new();
}
public sealed class AdminPromoProductDto
{
    public string SanPhamId { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
}

public sealed class AdminPromoCreateRequest
{
    [Required(ErrorMessage = "Tên chương trình không được để trống")]
    public string Ten { get; set; } = string.Empty;

    public string? MaCode { get; set; }
    public string? MoTa { get; set; }

    [Required]
    public int LoaiGiamGia { get; set; }

    [Required]
    public decimal GiaTriGiam { get; set; }

    public decimal GiamToiDa { get; set; }
    public decimal GiaTriToiThieuApDung { get; set; }

    [Required]
    public int SoLuongToiDa { get; set; }

    [Required]
    public DateTime NgayApDung { get; set; }

    [Required]
    public DateTime NgayKetThuc { get; set; }

    [Required]
    public string PhamVi { get; set; } = "all";

    public string? DanhMucId { get; set; }
    public List<string>? SanPhamIds { get; set; }
}
#endregion