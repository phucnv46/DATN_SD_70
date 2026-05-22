using DATN_70.Data;
using DATN_70.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DATN_70.Models.Enums.Enums;

namespace DATN_70.Services;

public class AutoCancelQROrdersService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoCancelQROrdersService> _logger;

    public AutoCancelQROrdersService(IServiceScopeFactory scopeFactory, ILogger<AutoCancelQROrdersService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dịch vụ tự động hủy đơn QR quá hạn đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var gioiHan = DateTime.Now.AddHours(-24);

                var donQuaHan = await dbContext.HoaDons
                    .Where(h => h.TrangThai == TrangThaiHoaDon.DangChoThanhToanQR
                             && h.NgayTao <= gioiHan)
                    .ToListAsync(stoppingToken);

                if (donQuaHan.Any())
                {
                    foreach (var don in donQuaHan)
                    {
                        don.TrangThai = TrangThaiHoaDon.DaHuy;
                        _logger.LogInformation("Đã hủy đơn QR quá hạn: {HoaDonID}", don.HoaDonID);
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý hủy đơn QR quá hạn.");
            }

            // Chờ 30 phút rồi quét tiếp
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}