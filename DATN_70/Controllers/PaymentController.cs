using DATN_70.Data;
using DATN_70.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models; 
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks; // Chứa class Webhook
using System;
using System.Threading.Tasks;

namespace DATN_70.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        public class PaymentRequestModel
        {
            public int Amount { get; set; }
        }
        private readonly PayOSClient _payOSClient;
        private readonly AppDbContext _dbContext;

        public PaymentController(PayOSClient payOSClient, AppDbContext dbContext )
        {
            _payOSClient = payOSClient;
            _dbContext = dbContext;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook([FromBody] Webhook webhookBody)
        {
            try
            {
                var verifiedData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);

                Console.WriteLine("=============================================");
                Console.WriteLine($"[TING TING] TIỀN ĐÃ VỀ TÀI KHOẢN!");
                Console.WriteLine($"Mã đơn hàng: {verifiedData.OrderCode}");
                Console.WriteLine($"Số tiền nhận: {verifiedData.Amount} VND");
                Console.WriteLine("=============================================");

                // 🌟 TỰ ĐỘNG CẬP NHẬT GẠCH NỢ DATABASE KHI NHẬN TÍN HIỆU TỪ CỔNG THANH TOÁN
                string currentOrderCodeStr = verifiedData.OrderCode.ToString();

                // Tìm kiếm thông tin thanh toán dựa trên mã tham chiếu gửi sang PayOS
                var chiTietThanhToan = await _dbContext.Set<ChiTietThanhToan>()
                    .FirstOrDefaultAsync(c => c.MaThamChieu == currentOrderCodeStr);

                if (chiTietThanhToan != null)
                {
                    // Đổi trạng thái giao dịch sang 1 (Thành công) theo Enum của bạn
                    chiTietThanhToan.TrangThai = DATN_70.Models.Enums.Enums.TrangThaiThanhToan.ThanhCong;

                    // Đồng thời tìm kiếm Hóa đơn tương ứng để đẩy trạng thái từ Chờ duyệt (0) sang Đã xác nhận (1)
                    var hoaDonGoc = await _dbContext.HoaDons
                        .FirstOrDefaultAsync(h => h.HoaDonID == chiTietThanhToan.HoaDonID);
                    if (hoaDonGoc != null)
                    {
                        hoaDonGoc.TrangThai = DATN_70.Models.Enums.Enums.TrangThaiHoaDon.DaXacNhan;
                    }

                    await _dbContext.SaveChangesAsync();
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WEBHOOK BÁO LỖI]: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] PaymentRequestModel req)
        {
            try
            {
                long orderCode = long.Parse(DateTime.Now.ToString("ddHHmmss"));

                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = req.Amount, // LẤY SỐ TIỀN THỰC TẾ TỪ QUẦY POS TRUYỀN LÊN
                    Description = $"WINTERPOS {orderCode}",
                    CancelUrl = "https://localhost:7220/Home/Cancel",
                    ReturnUrl = "https://localhost:7220/Home/Success"
                };

                var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

                return Ok(new
                {
                    success = true,
                    checkoutUrl = paymentLink.CheckoutUrl,
                    orderCode = orderCode
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI TẠO LINK QR]: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}