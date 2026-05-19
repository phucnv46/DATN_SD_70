using DATN_70.Controllers;
using DATN_70.Data;
using DATN_70.Models.Entities;
using DATN_70.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DATN_70.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminOrdersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 1. LẤY DANH SÁCH TOÀN BỘ ĐƠN HÀNG (CÓ LỌC THEO TRẠNG THÁI)
    [HttpGet]
    public async Task<IActionResult> GetAllOrders([FromQuery] string status = "all", CancellationToken cancellationToken = default)
    {
        var query = _dbContext.HoaDons
            .Include(h => h.KhachHang)
            .AsNoTracking();

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (TryMapStatusKey(status, out var orderStatus))
            {
                query = query.Where(h => h.TrangThai == orderStatus);
            }
            else
            {
                return BadRequest(new { message = "Trạng thái lọc không hợp lệ." });
            }
        }

        var orders = await query
            .OrderByDescending(h => h.NgayTao)
            .Select(h => new AdminOrderSummaryResponse
            {
                // ĐÃ SỬA: Xóa bỏ hoàn toàn thuộc tính thừa Id gây lỗi CS0117
                HoaDonId = h.HoaDonID,
                NgayTao = h.NgayTao,
                TenKhachHang = h.KhachHang != null ? h.KhachHang.Ten : "Khách ẩn danh",
                SoDienThoai = h.KhachHang != null ? h.KhachHang.SoDienThoai : string.Empty,
                ThanhTien = (decimal)h.ThanhTien,
                LoaiGiaoDich = h.LoaiGiaoDich == 0 ? "Online" : "Tại quầy",
                TrangThaiKey = GetStatusKey(h.TrangThai),
                TrangThaiLabel = GetStatusLabel(h.TrangThai)
            })
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    // 2. XEM CHI TIẾT MỘT ĐƠN HÀNG BẤT KỲ
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails([Required] string id, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.HoaDons
            .Include(h => h.KhachHang)
            .Include(h => h.DiaChi)
            .Include(h => h.HoaDonChiTiets)
                .ThenInclude(d => d.ChiTietSanPham)
                    .ThenInclude(p => p.SanPham)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.HoaDonID == id, cancellationToken);

        if (order is null)
        {
            return NotFound(new { message = $"Không tìm thấy đơn hàng với mã: {id}" });
        }

        var response = new AdminOrderDetailResponse
        {
            // ĐÃ SỬA: Xóa bỏ hoàn toàn thuộc tính thừa Id gây lỗi CS0117
            HoaDonId = order.HoaDonID,
            NgayTao = order.NgayTao,
            TongTienVAT = (decimal)order.TongTienVAT,
            TongTienGiamGia = (decimal)order.TongTienGiamGia,
            ThanhTien = (decimal)order.ThanhTien,
            LoaiGiaoDich = order.LoaiGiaoDich == 0 ? "Online" : "Tại quầy",
            TrangThaiKey = GetStatusKey(order.TrangThai),
            TrangThaiLabel = GetStatusLabel(order.TrangThai),
            GhiChu = order.GhiChu ?? string.Empty,
            KhachHang = new AdminOrderCustomerInfo
            {
                KhachHangId = order.KhachHangID,
                Ten = order.KhachHang?.Ten ?? "N/A",
                Email = order.KhachHang?.Email ?? string.Empty,
                SoDienThoai = order.KhachHang?.SoDienThoai ?? string.Empty
            },
            DiaChiGiaoHang = order.DiaChi != null ? new AdminOrderAddressInfo
            {
                TenNguoiNhan = order.DiaChi.TenNguoiNhan,
                SoDienThoaiNhan = order.DiaChi.SoDienThoaiNhan,
                DiaChiChiTiet = string.Join(", ", new[]
                {
                    AddressSerializer.ExtractStreet(order.DiaChi.PhuongXa),
                    AddressSerializer.ExtractWard(order.DiaChi.PhuongXa),
                    order.DiaChi.QuanHuyen,
                    order.DiaChi.TinhThanh
                }.Where(part => !string.IsNullOrWhiteSpace(part)))
            } : null,
            ChiTietSanPham = order.HoaDonChiTiets.Select(d => new AdminOrderItemResponse
            {
                ChiTietSanPhamId = d.ChiTietSanPhamID,
                TenSanPham = d.ChiTietSanPham?.SanPham?.Ten ?? "Sản phẩm đã bị xóa",
                KichCo = d.ChiTietSanPham?.KichCoID ?? "N/A",
                MauSac = d.ChiTietSanPham?.MauID ?? "N/A",
                SoLuong = d.SoLuong,
                DonGia = d.DonGia,
                TongTienMục = d.DonGia * d.SoLuong
            }).ToList()
        };

        return Ok(response);
    }

    // 3. CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (DUYỆT ĐƠN / CHUYỂN TRẠNG THÁI)
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus([Required] string id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.HoaDons.FirstOrDefaultAsync(h => h.HoaDonID == id, cancellationToken);
        if (order is null) return NotFound(new { message = $"Không tìm thấy đơn hàng với mã: {id}" });

        string action = request.Action?.Trim().ToLowerInvariant() ?? string.Empty;

        if (action == "next")
        {
            if (order.TrangThai == Enums.TrangThaiHoaDon.HoanThanh || order.TrangThai == Enums.TrangThaiHoaDon.DaHuy)
            {
                return BadRequest(new { message = "Đơn hàng đã kết thúc tiến trình." });
            }
            order.TrangThai = (Enums.TrangThaiHoaDon)((int)order.TrangThai + 1);
        }
        else if (action == "cancel")
        {
            if (order.TrangThai >= Enums.TrangThaiHoaDon.DangGiao)
            {
                return BadRequest(new { message = "Đơn hàng đang giao, không thể hủy." });
            }
            order.TrangThai = Enums.TrangThaiHoaDon.DaHuy;
        }
        else
        {
            return BadRequest(new { message = "Hành động không hợp lệ." });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Cập nhật tiến độ thành công.", currentStatusKey = GetStatusKey(order.TrangThai), currentStatusLabel = GetStatusLabel(order.TrangThai) });
    }

    // 4. LẤY DANH SÁCH TOÀN BỘ PHIẾU ĐỔI TRẢ
    [HttpGet("returns")]
    public async Task<IActionResult> GetAllReturns(CancellationToken cancellationToken = default)
    {
        var returns = await _dbContext.Set<PhieuDoiTra>()
            .Include(p => p.HoaDon) 
                .ThenInclude(h => h.KhachHang)
            .OrderByDescending(p => p.NgayTao)
            .Select(p => new {
                p.PhieuDoiTraID,
                p.HoaDonID,
                p.NgayTao,
                // ĐÃ SỬA: Kiểm tra an toàn sự tồn tại của hóa đơn và khách hàng
                TenKhachHang = (p.HoaDon != null && p.HoaDon.KhachHang != null) ? p.HoaDon.KhachHang.Ten : "Khách ẩn danh",
                p.TongTienHoan,
                TrangThai = (int)p.TrangThai,
                TrangThaiLabel = p.TrangThai == Enums.TrangThaiDoiTra.ChoXuLy ? "Chờ xử lý" :
                                 p.TrangThai == Enums.TrangThaiDoiTra.DaHoanTien_NhapKho ? "Đã nhập kho & hoàn tiền" : "Bị từ chối"
            })
            .ToListAsync(cancellationToken);

        return Ok(returns);
    }

    // 5. TẠO MỚI PHIẾU ĐỔI TRẢ HÀNG
    [HttpPost("returns")]
    public async Task<IActionResult> CreateReturnRequest([FromBody] AdminCreateReturnRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.HoaDons
            .Include(h => h.HoaDonChiTiets)
            .FirstOrDefaultAsync(h => h.HoaDonID == request.HoaDonId, cancellationToken);

        if (order == null) return NotFound(new { message = "Không tìm thấy hóa đơn tương ứng." });
        if (order.TrangThai != Enums.TrangThaiHoaDon.HoanThanh) return BadRequest(new { message = "Chỉ đơn hàng ở trạng thái [Thành công] mới được phép đổi trả!" });

        var phieuId = "RMA" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        decimal tongTienHoan = 0;

        var chiTietPhieus = new List<ChiTietDoiTra>();

        foreach (var item in request.Items)
        {
            var hdBf = order.HoaDonChiTiets.FirstOrDefault(d => d.ChiTietSanPhamID == item.ChiTietSanPhamId);
            if (hdBf == null || item.SoLuongTra > hdBf.SoLuong)
            {
                return BadRequest(new { message = "Số lượng hoàn trả vượt quá số lượng mua thực tế!" });
            }

            decimal giaTriHoanItem = hdBf.DonGia * item.SoLuongTra;
            tongTienHoan += giaTriHoanItem;

            chiTietPhieus.Add(new ChiTietDoiTra
            {
                ChiTietDoiTraID = Guid.NewGuid().ToString(),
                PhieuDoiTraID = phieuId,
                ChiTietSanPhamID = item.ChiTietSanPhamId,
                SoLuongTra = item.SoLuongTra,
                GiaTriHoanLai = giaTriHoanItem,
                LyDo = (Enums.LyDoDoiTra)item.LyDoKey
            });
        }

        var phieuDoiTra = new PhieuDoiTra
        {
            PhieuDoiTraID = phieuId,
            HoaDonID = request.HoaDonId,
            NgayTao = DateTime.Now,
            TrangThai = Enums.TrangThaiDoiTra.ChoXuLy,
            TongTienHoan = tongTienHoan,
            GhiChuAdmin = request.GhiChu
        };

        _dbContext.Set<PhieuDoiTra>().Add(phieuDoiTra);
        _dbContext.Set<ChiTietDoiTra>().AddRange(chiTietPhieus);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Tạo phiếu yêu cầu đổi trả thành công!", phieuDoiTraId = phieuId });
    }

    // 6. PHÊ DUYỆT PHIẾU ĐỔI TRẢ - TỰ ĐỘNG CỘNG HOÀN TỒN KHO VẬT LÝ
    [HttpPut("returns/{returnId}/approve")]
    public async Task<IActionResult> ApproveReturnRequest([Required] string returnId, CancellationToken cancellationToken = default)
    {
        var phieuDoiTra = await _dbContext.Set<PhieuDoiTra>()
            .Include(p => p.ChiTietDoiTras)
            .FirstOrDefaultAsync(p => p.PhieuDoiTraID == returnId, cancellationToken);

        if (phieuDoiTra == null) return NotFound(new { message = "Không tìm thấy phiếu đổi trả!" });
        if (phieuDoiTra.TrangThai != Enums.TrangThaiDoiTra.ChoXuLy) return BadRequest(new { message = "Phiếu đổi trả này đã được xử lý tiến trình trước đó." });

        phieuDoiTra.TrangThai = Enums.TrangThaiDoiTra.DaHoanTien_NhapKho;
        var hoaDonGoc = await _dbContext.HoaDons.FirstOrDefaultAsync(h => h.HoaDonID == phieuDoiTra.HoaDonID, cancellationToken);
        if (hoaDonGoc != null)
        {
            hoaDonGoc.TrangThai = Enums.TrangThaiHoaDon.DaDoiTra;
        }
        foreach (var item in phieuDoiTra.ChiTietDoiTras)
        {
            var chiTietSP = await _dbContext.ChiTietSanPhams
                .FirstOrDefaultAsync(ct => ct.ChiTietSanPhamID == item.ChiTietSanPhamID, cancellationToken);

            if (chiTietSP != null)
            {
                chiTietSP.SoLuongTonKho += item.SoLuongTra;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Duyệt yêu cầu thành công! Số lượng hàng hóa đã được hoàn trả về hệ thống kho lưu trữ." });
    }
    // 7. TỪ CHỐI YÊU CẦU ĐỔI TRẢ CỦA KHÁCH HÀNG (API MỚI BỔ SUNG)
    [HttpPut("returns/{returnId}/reject")]
    public async Task<IActionResult> RejectReturnRequest([Required] string returnId, CancellationToken cancellationToken = default)
    {
        var phieuDoiTra = await _dbContext.Set<PhieuDoiTra>()
            .FirstOrDefaultAsync(p => p.PhieuDoiTraID == returnId, cancellationToken);

        if (phieuDoiTra == null) return NotFound(new { message = "Không tìm thấy phiếu đổi trả!" });
        if (phieuDoiTra.TrangThai != Enums.TrangThaiDoiTra.ChoXuLy) return BadRequest(new { message = "Phiếu này đã được xử lý tiến trình từ trước." });

        // Chuyển trạng thái sang Bị từ chối (2), không can thiệp vào số lượng tồn kho vật lý
        phieuDoiTra.TrangThai = Enums.TrangThaiDoiTra.TuChoi;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Đã từ chối yêu cầu đổi trả của khách hàng." });
    }

    #region Helper Methods Mappings
    private static bool TryMapStatusKey(string statusKey, out Enums.TrangThaiHoaDon orderStatus)
    {
        switch (statusKey?.Trim().ToLowerInvariant())
        {
            case "pending": orderStatus = Enums.TrangThaiHoaDon.ChoDuyet; return true;
            case "confirmed": orderStatus = Enums.TrangThaiHoaDon.DaXacNhan; return true;
            case "preparing": orderStatus = Enums.TrangThaiHoaDon.DangChuanBi; return true;
            case "shipping": orderStatus = Enums.TrangThaiHoaDon.DangGiao; return true;
            case "success": orderStatus = Enums.TrangThaiHoaDon.HoanThanh; return true;
            case "cancelled": orderStatus = Enums.TrangThaiHoaDon.DaHuy; return true;
            default: orderStatus = Enums.TrangThaiHoaDon.ChoDuyet; return false;
        }
    }

    private static string GetStatusKey(Enums.TrangThaiHoaDon status)
    {
        return status switch
        {
            Enums.TrangThaiHoaDon.ChoDuyet => "pending",
            Enums.TrangThaiHoaDon.DaXacNhan => "confirmed",
            Enums.TrangThaiHoaDon.DangChuanBi => "preparing",
            Enums.TrangThaiHoaDon.DangGiao => "shipping",
            Enums.TrangThaiHoaDon.HoanThanh => "success",
            Enums.TrangThaiHoaDon.DaHuy => "cancelled",
            Enums.TrangThaiHoaDon.DaDoiTra => "returned",
            _ => "pending"
        };
    }

    private static string GetStatusLabel(Enums.TrangThaiHoaDon status)
    {
        return status switch
        {
            Enums.TrangThaiHoaDon.ChoDuyet => "Chờ xác nhận",
            Enums.TrangThaiHoaDon.DaXacNhan => "Đã xác nhận",
            Enums.TrangThaiHoaDon.DangChuanBi => "Đang chuẩn bị",
            Enums.TrangThaiHoaDon.DangGiao => "Đang giao",
            Enums.TrangThaiHoaDon.HoanThanh => "Thành công",
            Enums.TrangThaiHoaDon.DaHuy => "Đã hủy",
            Enums.TrangThaiHoaDon.DaDoiTra => "Đã đổi trả",
            _ => "Chờ xác nhận"
        };
    }
    #endregion
}

#region ĐỒNG BỘ TOÀN BỘ CÁC LỚP DATA TRANSFER OBJECTS (DTOS) KHÔNG THIẾU MẢNH NÀO
public sealed class UpdateStatusRequest
{
    [Required(ErrorMessage = "Hành động xử lý là bắt buộc.")]
    public string Action { get; set; } = string.Empty;
}

public sealed class AdminOrderSummaryResponse
{
    public string HoaDonId { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public string TenKhachHang { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public decimal ThanhTien { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThaiKey { get; set; } = string.Empty;
    public string TrangThaiLabel { get; set; } = string.Empty;
}

public sealed class AdminOrderDetailResponse
{
    public string HoaDonId { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public decimal TongTienVAT { get; set; }
    public decimal TongTienGiamGia { get; set; }
    public decimal ThanhTien { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThaiKey { get; set; } = string.Empty;
    public string TrangThaiLabel { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public AdminOrderCustomerInfo KhachHang { get; set; } = new();
    public AdminOrderAddressInfo? DiaChiGiaoHang { get; set; }
    public List<AdminOrderItemResponse> ChiTietSanPham { get; set; } = new();
}

public sealed class AdminOrderCustomerInfo
{
    public string KhachHangId { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
}

public sealed class AdminOrderAddressInfo
{
    public string TenNguoiNhan { get; set; } = string.Empty;
    public string SoDienThoaiNhan { get; set; } = string.Empty;
    public string DiaChiChiTiet { get; set; } = string.Empty;
}

public sealed class AdminOrderItemResponse
{
    public string ChiTietSanPhamId { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string KichCo { get; set; } = string.Empty;
    public string MauSac { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal TongTienMục { get; set; }
}

public class AdminCreateReturnRequest
{
    [Required]
    public string HoaDonId { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public List<AdminCreateReturnItem> Items { get; set; } = new();
}

public class AdminCreateReturnItem
{
    [Required]
    public string ChiTietSanPhamId { get; set; } = string.Empty;
    [Required]
    public int SoLuongTra { get; set; }
    [Required]
    public int LyDoKey { get; set; }
}
#endregion