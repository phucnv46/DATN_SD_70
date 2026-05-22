using DATN_70.Attributes;
using DATN_70.Data;
using DATN_70.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static DATN_70.Models.Enums.Enums;

namespace DATN_70.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
[CustomAuthorize("R01", "R02")]
public class AdminReportsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminReportsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 1. TỔNG QUAN DASHBOARD
    // 1. TỔNG QUAN DASHBOARD
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.HoaDons.AsQueryable();
        if (fromDate.HasValue)
            query = query.Where(h => h.NgayTao >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(h => h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        // Tổng doanh thu từ các đơn hàng đã Hoàn thành
        var totalRevenue = await query
            .Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh)
            .SumAsync(h => h.ThanhTien, cancellationToken);

        // Số đơn hàng theo từng trạng thái
        var totalOrders = await query.CountAsync(cancellationToken);
        var successOrders = await query
            .Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh)
            .CountAsync(cancellationToken);
        var cancelledOrders = await query
            .Where(h => h.TrangThai == TrangThaiHoaDon.DaHuy)
            .CountAsync(cancellationToken);
        var pendingOrders = await query
            .Where(h => h.TrangThai == TrangThaiHoaDon.ChoDuyet)
            .CountAsync(cancellationToken);
        var processingOrders = await query
            .Where(h => h.TrangThai == TrangThaiHoaDon.DaXacNhan
                     || h.TrangThai == TrangThaiHoaDon.DangChuanBi
                     || h.TrangThai == TrangThaiHoaDon.DangGiao)
            .CountAsync(cancellationToken);

        // Tổng số sản phẩm đã bán (từ các đơn thành công)
        var totalItemsSold = await _dbContext.HoaDonChiTiets
            .Where(ct => ct.HoaDon.TrangThai == TrangThaiHoaDon.HoanThanh
                      && (fromDate == null || ct.HoaDon.NgayTao >= fromDate)
                      && (toDate == null || ct.HoaDon.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1)))
            .SumAsync(ct => ct.SoLuong, cancellationToken);

        return Ok(new DashboardSummaryResponse
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            SuccessOrders = successOrders,
            CancelledOrders = cancelledOrders,
            PendingOrders = pendingOrders,
            ProcessingOrders = processingOrders,
            TotalItemsSold = totalItemsSold
        });
    }

    // 2. DOANH THU THEO THỜI GIAN (DÙNG CHO BIỂU ĐỒ ĐƯỜNG)
    // 2. DOANH THU THEO THỜI GIAN (DÙNG CHO BIỂU ĐỒ ĐƯỜNG)
    [HttpGet("revenue-over-time")]
    public async Task<IActionResult> GetRevenueOverTime(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var endDate = toDate?.Date.AddDays(1) ?? DateTime.Now.Date.AddDays(1);
        var startDate = fromDate?.Date ?? DateTime.Now.Date.AddDays(-6);

        var revenueData = await _dbContext.HoaDons
            .Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh
                     && h.NgayTao >= startDate
                     && h.NgayTao < endDate)
            .GroupBy(h => h.NgayTao.Date)
            .Select(g => new RevenueByDayResponse
            {
                Date = g.Key,
                Revenue = g.Sum(h => h.ThanhTien),
                OrderCount = g.Count()
            })
            .OrderBy(r => r.Date)
            .ToListAsync(cancellationToken);

        var result = new List<RevenueByDayResponse>();
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            var dayData = revenueData.FirstOrDefault(r => r.Date == date);
            result.Add(dayData ?? new RevenueByDayResponse
            {
                Date = date,
                Revenue = 0,
                OrderCount = 0
            });
        }

        return Ok(result);
    }

    // 3. TOP SẢN PHẨM BÁN CHẠY (DÙNG CHO BIỂU ĐỒ CỘT)
    // 3. TOP SẢN PHẨM BÁN CHẠY (DÙNG CHO BIỂU ĐỒ CỘT)
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery, Range(5, 50)] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.HoaDonChiTiets
            .Where(ct => ct.HoaDon.TrangThai == TrangThaiHoaDon.HoanThanh);
        if (fromDate.HasValue)
            query = query.Where(ct => ct.HoaDon.NgayTao >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(ct => ct.HoaDon.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        var topProducts = await query
            .GroupBy(ct => new { ct.ChiTietSanPhamID, ct.ChiTietSanPham.SanPham.Ten })
            .Select(g => new TopProductResponse
            {
                ProductName = g.Key.Ten,
                TotalSold = g.Sum(ct => ct.SoLuong),
                TotalRevenue = g.Sum(ct => ct.SoLuong * ct.DonGia)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return Ok(topProducts);
    }

    #region Data Transfer Objects (DTOs) cho Report
    public class DashboardSummaryResponse
    {
        public double TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int SuccessOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int TotalItemsSold { get; set; }
    }

    public class RevenueByDayResponse
    {
        public DateTime Date { get; set; }
        public double Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductResponse
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
#endregion