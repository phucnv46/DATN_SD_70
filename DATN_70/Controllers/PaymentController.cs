using DATN_70.Data;
using DATN_70.Models.Entities;
using DATN_70.Models.Enums;
using DATN_70.Models.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models; 
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks; // Chứa class Webhook
using System;
using System.Text.Json;
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
        public async Task<IActionResult> ReceiveWebhook([FromBody] PayOS.Models.Webhooks.Webhook webhookBody)
        {
            try
            {
                // 1. Xác thực dữ liệu từ PayOS
                dynamic verifiedData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);
                Console.WriteLine("=============================================");
                Console.WriteLine($"[TING TING] TIỀN ĐÃ VỀ TÀI KHOẢN!");
                Console.WriteLine($"Mã đơn hàng: {verifiedData.OrderCode}");
                Console.WriteLine($"Số tiền nhận: {verifiedData.Amount} VND");
                Console.WriteLine("=============================================");

                string currentOrderCodeStr = verifiedData.OrderCode.ToString();

                // 2. Tìm bản ghi thanh toán tương ứng
                var chiTietThanhToan = await _dbContext.Set<ChiTietThanhToan>()
                    .FirstOrDefaultAsync(c => c.MaThamChieu == currentOrderCodeStr);

                if (chiTietThanhToan == null)
                {
                    // Kiểm tra xem có phải là intent QR không
                    Console.WriteLine($"[WEBHOOK] Không tìm thấy giao dịch với mã: {currentOrderCodeStr}");
                    return Ok(new { success = true, message = "Không tìm thấy giao dịch tương ứng." });
                }

                // 3. Chống replay: nếu đã thành công rồi thì thôi
                if (chiTietThanhToan.TrangThai == Enums.TrangThaiThanhToan.ThanhCong)
                {
                    Console.WriteLine($"[WEBHOOK] Giao dịch {currentOrderCodeStr} đã được xử lý trước đó. Bỏ qua.");
                    return Ok(new { success = true, message = "Giao dịch đã được xử lý." });
                }

                // 4. Cập nhật trạng thái thanh toán
                chiTietThanhToan.TrangThai = Enums.TrangThaiThanhToan.ThanhCong;

                // 5. Xử lý hóa đơn: tìm và chuyển trạng thái từ "Chờ thanh toán QR" sang "Đã xác nhận"
                var hoaDon = await _dbContext.HoaDons
                    .FirstOrDefaultAsync(h => h.HoaDonID == chiTietThanhToan.HoaDonID);

                if (hoaDon != null)
                {
                    // Kiểm tra xem đơn hàng có đang ở trạng thái có thể xác nhận không
                    bool canConfirm = hoaDon.TrangThai == Enums.TrangThaiHoaDon.ChoDuyet ||
                                      hoaDon.TrangThai == Enums.TrangThaiHoaDon.DangChoThanhToanQR;

                    if (canConfirm)
                    {
                        // Nếu là đơn QR (trạng thái 7), cần trừ kho trước khi đổi trạng thái
                        if (hoaDon.TrangThai == Enums.TrangThaiHoaDon.DangChoThanhToanQR)
                        {
                            var chiTietHoaDon = await _dbContext.HoaDonChiTiets
                                .Where(hdct => hdct.HoaDonID == hoaDon.HoaDonID)
                                .ToListAsync();

                            foreach (var item in chiTietHoaDon)
                            {
                                var ctsp = await _dbContext.ChiTietSanPhams
                                    .FirstOrDefaultAsync(ct => ct.ChiTietSanPhamID == item.ChiTietSanPhamID);
                                if (ctsp != null)
                                {
                                    ctsp.SoLuongTonKho -= item.SoLuong;
                                }
                            }
                        }

                        // Sau khi đã trừ kho (nếu cần), mới chính thức chuyển trạng thái
                        hoaDon.TrangThai = Enums.TrangThaiHoaDon.DaXacNhan;
                        Console.WriteLine($"[WEBHOOK] Đơn hàng {hoaDon.HoaDonID} đã được xác nhận thành công.");
                    }
                    else
                    {
                        Console.WriteLine($"[WEBHOOK] Đơn hàng {hoaDon.HoaDonID} không ở trạng thái có thể xác nhận. Trạng thái hiện tại: {hoaDon.TrangThai}");
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Cập nhật thanh toán thành công." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WEBHOOK BÁO LỖI]: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        //[HttpPost("create-payment")]
        //public async Task<IActionResult> CreatePaymentUrl([FromBody] PaymentRequestModel req)
        //{
        //    try
        //    {
        //        long orderCode = long.Parse(DateTime.Now.ToString("ddHHmmss"));

        //        var paymentRequest = new CreatePaymentLinkRequest
        //        {
        //            OrderCode = orderCode,
        //            Amount = req.Amount, // LẤY SỐ TIỀN THỰC TẾ TỪ QUẦY POS TRUYỀN LÊN
        //            Description = $"WINTERPOS {orderCode}",
        //            CancelUrl = "https://localhost:7220/Home/Cancel",
        //            ReturnUrl = "https://localhost:7220/Home/Success"
        //        };

        //        var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

        //        return Ok(new
        //        {
        //            success = true,
        //            checkoutUrl = paymentLink.CheckoutUrl,
        //            orderCode = orderCode
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[LỖI TẠO LINK QR]: {ex.Message}");
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}
        [HttpPost("create-online-payment")]
        public async Task<IActionResult> CreateOnlinePaymentUrl(
     [FromBody] CreateOnlinePaymentRequest request,
     CancellationToken cancellationToken)
        {
            // 1. Yêu cầu load Session nếu chưa có sẵn
            await HttpContext.Session.LoadAsync();
            var currentUserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập để thực hiện chức năng này." });
            }

            // 2. TÌM HÓA ĐƠN VÀ KIỂM TRA QUYỀN SỞ HỮU
            var hoaDon = await _dbContext.HoaDons
                .Include(h => h.ChiTietThanhToans) // Để kiểm tra xem đã có thanh toán nào chưa
                .FirstOrDefaultAsync(h => h.HoaDonID == request.HoaDonID, cancellationToken);

            if (hoaDon == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn." });
            }



            // Kiểm tra trạng thái: Chỉ tạo link khi hóa đơn ở trạng thái Chờ duyệt
            if (hoaDon.TrangThai != Enums.TrangThaiHoaDon.ChoDuyet && hoaDon.TrangThai != Enums.TrangThaiHoaDon.DangChoThanhToanQR)
            {
                return BadRequest(new { message = $"Đơn hàng không ở trạng thái chờ thanh toán. Trạng thái hiện tại: {hoaDon.TrangThai}" });
            }

            // Xóa các yêu cầu thanh toán cũ chưa thành công (nếu có)
            if (hoaDon.ChiTietThanhToans != null)
            {
                var pendingPayments = hoaDon.ChiTietThanhToans
                    .Where(ct => ct.TrangThai == Enums.TrangThaiThanhToan.ThatBai)
                    .ToList();
                foreach (var pp in pendingPayments)
                {
                    _dbContext.Set<ChiTietThanhToan>().Remove(pp);
                }
            }

            // 3. SINH MÃ ORDERCODE DUY NHẤT (AN TOÀN, KHÔNG TRÙNG)
            long orderCode = GenerateUniquePayOSOrderCode();

            // 4. TẠO BẢN GHI CHI TIẾT THANH TOÁN (TRẠNG THÁI CHỜ)
            var chiTietThanhToan = new ChiTietThanhToan
            {
                ChiTietThanhToanID = Guid.NewGuid().ToString(),
                HoaDonID = hoaDon.HoaDonID,
                Ten = "Thanh toán QR Online",
                SoTien = (decimal)hoaDon.ThanhTien,
                MaThamChieu = orderCode.ToString(),
                ThoiGianThanhToan = DateTime.Now,
                TrangThai = Enums.TrangThaiThanhToan.ThatBai, // Chưa thanh toán
                PhuongThucThanhToanID = "CASH" // Hoặc một mã riêng cho QR Online nếu có
            };
            _dbContext.Set<ChiTietThanhToan>().Add(chiTietThanhToan);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 5. GỌI PAYOS ĐỂ TẠO LINK THANH TOÁN
            try
            {
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = (int)hoaDon.ThanhTien,
                    Description = $"DH{orderCode}"[..Math.Min(25, orderCode.ToString().Length + 2)],
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
                // Nếu gọi PayOS thất bại, xóa bản ghi ChiTietThanhToan vừa tạo
                _dbContext.Set<ChiTietThanhToan>().Remove(chiTietThanhToan);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Trả lỗi chi tiết ra ngoài để test (CHỈ DÙNG KHI TEST, sau này phải ẩn đi)
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Hàm sinh OrderCode an toàn (đặt trong cùng controller)
        private long GenerateUniquePayOSOrderCode()
        {
            // Sử dụng timestamp đến mili giây + 3 chữ số random cuối để đảm bảo duy nhất
            var timestampPart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var randomPart = new Random().Next(100, 999).ToString();
            return long.Parse(timestampPart + randomPart);
        }
        [HttpDelete("cancel-qr/{hoaDonId}")]
        public async Task<IActionResult> CancelQRPayment(string hoaDonId)
        {
            // Xóa link thanh toán QR nếu có
            var chiTiet = await _dbContext.Set<ChiTietThanhToan>()
                .FirstOrDefaultAsync(c => c.HoaDonID == hoaDonId && c.TrangThai == Enums.TrangThaiThanhToan.ThatBai);
            if (chiTiet != null)
            {
                _dbContext.Set<ChiTietThanhToan>().Remove(chiTiet);
            }

            // Nếu đơn hàng đang ở trạng thái 7 (Chờ thanh toán QR), hủy luôn đơn hàng
            var hoaDon = await _dbContext.HoaDons.FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId);
            if (hoaDon != null && hoaDon.TrangThai == Enums.TrangThaiHoaDon.DangChoThanhToanQR)
            {
                hoaDon.TrangThai = Enums.TrangThaiHoaDon.DaHuy;
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { success = true });
        }
        [HttpPost("create-qr-intent")]
        public async Task<IActionResult> CreateQRPaymentIntent([FromBody] PlaceOrderRequest request)
        {
            // Lưu toàn bộ request vào Session để webhook dùng lại
            HttpContext.Session.SetString("LastPlaceOrderRequest", JsonSerializer.Serialize(request));

            // Sinh mã OrderCode
            long orderCode = GenerateUniquePayOSOrderCode();

            // Tạo link PayOS (dùng số tiền tạm, hoặc tính từ giỏ hàng)
            int tempAmount = 1000; // Có thể tính thật từ cartItems
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = tempAmount,
                Description = $"QR{orderCode}".Substring(0, 25),
                CancelUrl = "https://localhost:7220/Home/Cancel",
                ReturnUrl = "https://localhost:7220/Home/Success"
            };
            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

            return Ok(new { success = true, checkoutUrl = paymentLink.CheckoutUrl, orderCode });
        }
    }
}