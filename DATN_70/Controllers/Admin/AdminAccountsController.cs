using DATN_70.Attributes;
using DATN_70.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DATN_70.Controllers.Admin;

[ApiController]
[Route("api/admin/accounts")]
[CustomAuthorize("R01", "R02")]
public sealed class AdminAccountsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminAccountsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 1. LẤY DANH SÁCH TOÀN BỘ TÀI KHOẢN (CÓ BỘ LỌC SEARCH & FILTER)
    // 1. LẤY DANH SÁCH TÀI KHOẢN (TỰ ĐỘNG NHẬN DIỆN KHÁCH HÀNG / NHÂN VIÊN)
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? roleId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TaiKhoans
            .Include(t => t.KhachHang)
            .Include(t => t.NhanVien) // BỔ SUNG: Nhúng thêm bảng Nhân viên
            .AsNoTracking();

        // Lọc theo từ khóa tìm kiếm
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(t => t.Email.ToLower().Contains(term) ||
                                    (t.KhachHang != null && t.KhachHang.Ten.ToLower().Contains(term)) ||
                                    (t.NhanVien != null && t.NhanVien.Ten.ToLower().Contains(term)) ||
                                    (t.KhachHang != null && t.KhachHang.SoDienThoai.Contains(term)) ||
                                    (t.NhanVien != null && t.NhanVien.SoDienThoai.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.TrangThai == status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(roleId))
        {
            query = query.Where(t => t.VaiTroID == roleId.Trim());
        }

        var accounts = await query
            .OrderBy(t => t.Email)
            .Select(t => new AdminAccountSummaryResponse
            {
                TaiKhoanId = t.TaiKhoanID,
                Email = t.Email,
                TrangThai = t.TrangThai ?? "Hoạt động",
                VaiTroId = t.VaiTroID,
                // Ưu tiên lấy tên nhân viên trước, nếu không có thì lấy tên khách hàng
                TenChuTaiKhoan = t.NhanVien != null ? t.NhanVien.Ten :
                                 (t.KhachHang != null ? t.KhachHang.Ten : "Chưa cập nhật"),
                SoDienThoai = t.NhanVien != null ? t.NhanVien.SoDienThoai :
                              (t.KhachHang != null ? t.KhachHang.SoDienThoai : string.Empty)
            })
            .ToListAsync(cancellationToken);

        return Ok(accounts);
    }

    // 2. XEM CHI TIẾT MỘT TÀI KHOẢN KÈM HỒ SƠ KHÁCH HÀNG
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountDetails([Required] string id, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.TaiKhoans
            .Include(t => t.KhachHang)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TaiKhoanID == id, cancellationToken);

        if (account is null)
        {
            return NotFound(new { message = $"Không tìm thấy tài khoản với mã ID: {id}" });
        }

        var response = new AdminAccountDetailResponse
        {
            TaiKhoanId = account.TaiKhoanID,
            Email = account.Email,
            TrangThai = account.TrangThai ?? "Hoạt động",
            VaiTroId = account.VaiTroID,
            KhachHangProfile = account.KhachHang != null ? new AdminCustomerProfileDto
            {
                KhachHangId = account.KhachHang.KhachHangID,
                Ten = account.KhachHang.Ten,
                EmailProfile = account.KhachHang.Email ?? string.Empty,
                SoDienThoai = account.KhachHang.SoDienThoai,

                GioiTinhText = (int)account.KhachHang.GioiTinh == 0 ? "Nam" : (int)account.KhachHang.GioiTinh == 1 ? "Nữ" : "Khác",

                DiaChiText = account.KhachHang.DiaChi ?? string.Empty,
                DiemTichLuy = account.KhachHang.DiemTichLuy
            } : null 
        };

        return Ok(response);
    }

    // 3. CẬP NHẬT TRẠNG THÁI TÀI KHOẢN (KHÓA HOẶC MỞ KHÓA)
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAccountStatus([Required] string id, [FromBody] AdminUpdateStatusAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.TaiKhoans.FirstOrDefaultAsync(t => t.TaiKhoanID == id, cancellationToken);
        if (account is null)
        {
            return NotFound(new { message = $"Không tìm thấy tài khoản với mã ID: {id}" });
        }

        // Cập nhật giá trị trạng thái thực tế vào cột dữ liệu
        account.TrangThai = request.TrangThai.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = $"Cập nhật trạng thái tài khoản {account.Email} thành công.",
            taiKhoanId = account.TaiKhoanID,
            currentStatus = account.TrangThai
        });
    }
}

#region Data Transfer Objects (DTOs) dành riêng cho phân hệ Admin Account
public sealed class AdminUpdateStatusAccountRequest
{
    [Required(ErrorMessage = "Trạng thái tài khoản không được để trống.")]
    [MaxLength(50)]
    public string TrangThai { get; set; } = string.Empty; // "Hoạt động" hoặc "Bị khóa"
}

public sealed class AdminAccountSummaryResponse
{
    public string TaiKhoanId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string VaiTroId { get; set; } = string.Empty;
    public string TenChuTaiKhoan { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
}

public sealed class AdminAccountDetailResponse
{
    public string TaiKhoanId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string VaiTroId { get; set; } = string.Empty;
    public AdminCustomerProfileDto? KhachHangProfile { get; set; }
}

public sealed class AdminCustomerProfileDto
{
    public string KhachHangId { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string EmailProfile { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string GioiTinhText { get; set; } = string.Empty;
    public string DiaChiText { get; set; } = string.Empty;
    public int DiemTichLuy { get; set; }
}
#endregion