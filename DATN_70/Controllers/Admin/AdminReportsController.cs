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
    // 4. PHÂN TÍCH KHÁCH HÀNG
    [HttpGet("customer-stats")]
    public async Task<IActionResult> GetCustomerStats(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.HoaDons.Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh);
        if (fromDate.HasValue) query = query.Where(h => h.NgayTao >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(h => h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        var hoaDonList = await query.Select(h => new { h.KhachHangID, h.ThanhTien }).ToListAsync(cancellationToken);
        var totalCustomers = hoaDonList.Select(h => h.KhachHangID).Distinct().Count();
        var customerOrderCounts = hoaDonList.GroupBy(h => h.KhachHangID)
            .Select(g => new { KhachHangID = g.Key, OrderCount = g.Count() }).ToList();
        var returningCustomers = customerOrderCounts.Count(c => c.OrderCount >= 2);
        var returningRate = totalCustomers > 0 ? (double)returningCustomers / totalCustomers * 100 : 0;

        var topCustomers = await _dbContext.KhachHangs
            .Where(k => hoaDonList.Select(h => h.KhachHangID).Distinct().Contains(k.KhachHangID))
            .Select(k => new TopCustomerResponse
            {
                CustomerName = k.Ten ?? "Chưa cập nhật",
                TotalSpent = _dbContext.HoaDons
                    .Where(h => h.KhachHangID == k.KhachHangID && h.TrangThai == TrangThaiHoaDon.HoanThanh
                        && (fromDate == null || h.NgayTao >= fromDate) && (toDate == null || h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1)))
                    .Sum(h => h.ThanhTien),
                OrderCount = _dbContext.HoaDons
                    .Count(h => h.KhachHangID == k.KhachHangID && h.TrangThai == TrangThaiHoaDon.HoanThanh
                        && (fromDate == null || h.NgayTao >= fromDate) && (toDate == null || h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1)))
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(10)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            totalCustomers,
            returningRate = Math.Round(returningRate, 1),
            topCustomers
        });
    }

    // 5. HIỆU QUẢ KHUYẾN MÃI
    [HttpGet("promotion-stats")]
    public async Task<IActionResult> GetPromotionStats(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.HoaDons.Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh);
        if (fromDate.HasValue) query = query.Where(h => h.NgayTao >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(h => h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        var revenueWithPromo = await query.Where(h => !string.IsNullOrEmpty(h.KhuyenMaiID)).SumAsync(h => h.ThanhTien, cancellationToken);
        var revenueWithoutPromo = await query.Where(h => string.IsNullOrEmpty(h.KhuyenMaiID)).SumAsync(h => h.ThanhTien, cancellationToken);

        var topPromotions = await _dbContext.KhuyenMais
            .Where(k => k.HoaDons.Any(h => h.TrangThai == TrangThaiHoaDon.HoanThanh
                && (fromDate == null || h.NgayTao >= fromDate) && (toDate == null || h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1))))
            .Select(k => new TopPromotionResponse
            {
                PromotionName = k.Ten,
                Code = k.MaCode ?? "Tự động",
                TimesUsed = k.HoaDons.Count(h => h.TrangThai == TrangThaiHoaDon.HoanThanh
                    && (fromDate == null || h.NgayTao >= fromDate) && (toDate == null || h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1))),
                TotalDiscount = k.HoaDons.Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh
                    && (fromDate == null || h.NgayTao >= fromDate) && (toDate == null || h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1)))
                    .Sum(h => h.TongTienGiamGia)
            })
            .OrderByDescending(p => p.TimesUsed)
            .Take(10)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            revenueWithPromo,
            revenueWithoutPromo,
            topPromotions
        });
    }

    // 6. SO SÁNH KÊNH BÁN
    [HttpGet("channel-stats")]
    public async Task<IActionResult> GetChannelStats(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.HoaDons.Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh);
        if (fromDate.HasValue) query = query.Where(h => h.NgayTao >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(h => h.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        var onlineRevenue = await query.Where(h => h.LoaiGiaoDich == LoaiGiaoDich.Online).SumAsync(h => h.ThanhTien, cancellationToken);
        var posRevenue = await query.Where(h => h.LoaiGiaoDich == LoaiGiaoDich.PosTaiQuay).SumAsync(h => h.ThanhTien, cancellationToken);

        // Doanh thu theo ngày của từng kênh (7 ngày gần nhất)
        var endDate = DateTime.Now.Date.AddDays(1);
        var startDate = DateTime.Now.Date.AddDays(-6);
        var dailyStats = await _dbContext.HoaDons
            .Where(h => h.TrangThai == TrangThaiHoaDon.HoanThanh && h.NgayTao >= startDate && h.NgayTao < endDate)
            .GroupBy(h => new { h.NgayTao.Date, h.LoaiGiaoDich })
            .Select(g => new
            {
                Date = g.Key.Date,
                Channel = g.Key.LoaiGiaoDich == LoaiGiaoDich.Online ? "Online" : "POS",
                Revenue = g.Sum(h => h.ThanhTien)
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            onlineRevenue,
            posRevenue,
            dailyStats
        });
    }

    // 7. PHÂN TÍCH DANH MỤC
    [HttpGet("category-stats")]
    public async Task<IActionResult> GetCategoryStats(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.HoaDonChiTiets
            .Where(ct => ct.HoaDon.TrangThai == TrangThaiHoaDon.HoanThanh);
        if (fromDate.HasValue) query = query.Where(ct => ct.HoaDon.NgayTao >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(ct => ct.HoaDon.NgayTao <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        var categoryStats = await query
            .GroupBy(ct => ct.ChiTietSanPham.SanPham.DanhMuc.Ten)
            .Select(g => new CategoryStatsResponse
            {
                CategoryName = g.Key,
                Revenue = g.Sum(ct => ct.SoLuong * ct.DonGia)
            })
            .OrderByDescending(c => c.Revenue)
            .ToListAsync(cancellationToken);

        return Ok(categoryStats);
    }

    // 8. HÀNG TỒN KHO
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory(CancellationToken cancellationToken = default)
    {
        var inventory = await _dbContext.ChiTietSanPhams
            .Include(ct => ct.SanPham)
            .GroupBy(ct => new { ct.SanPhamID, ct.SanPham.Ten })
            .Select(g => new InventoryResponse
            {
                ProductName = g.Key.Ten,
                TotalStock = g.Sum(ct => ct.SoLuongTonKho)
            })
            .OrderByDescending(i => i.TotalStock)
            .Take(10)
            .ToListAsync(cancellationToken);

        return Ok(inventory);
    }

    // === THÊM CÁC DTO MỚI ===
    public class TopCustomerResponse
    {
        public string CustomerName { get; set; } = string.Empty;
        public double TotalSpent { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopPromotionResponse
    {
        public string PromotionName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public int TimesUsed { get; set; }
        public double TotalDiscount { get; set; }
    }

    public class CategoryStatsResponse
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class InventoryResponse
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalStock { get; set; }
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