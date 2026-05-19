using DATN_70.Attributes;
using DATN_70.Data;
using DATN_70.Models.Entities;
using DATN_70.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
namespace DATN_70.Controllers.Admin;

[ApiController]
[Route("api/admin/pos")]
[CustomAuthorize("R01", "R02")]
public class AdminPOSController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminPOSController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ==========================================
    // 1. QUẢN LÝ SẢN PHẨM TẠI QUẦY
    // ==========================================

    [HttpGet("products")]
    public async Task<IActionResult> GetProductsForPOS(CancellationToken cancellationToken)
    {
        // Chỉ lấy những sản phẩm đang có tồn kho > 0 để tránh bán âm
        var products = await _dbContext.ChiTietSanPhams
            .Include(ct => ct.SanPham)
            .ThenInclude(sp => sp.HinhAnhSanPhams) // ĐỒNG BỘ: Gọi thêm bảng hình ảnh sản phẩm
            .Include(ct => ct.KichCo)
            .Include(ct => ct.Mau)
            .Where(ct => ct.SoLuongTonKho > 0)
            .Select(ct => new PosProductDto
            {
                ChiTietSanPhamID = ct.ChiTietSanPhamID,
                TenSanPham = ct.SanPham.Ten,
                SKU = ct.SKU,
                GiaBan = ct.GiaNiemYet,
                MucVAT = ct.SanPham.MucVAT,
                KichCo = ct.KichCo != null ? ct.KichCo.Ten : "N/A",
                MauSac = ct.Mau != null ? ct.Mau.Ten : "N/A",
                SoLuongTonKho = ct.SoLuongTonKho,
                HinhAnhUrl = ct.SanPham.HinhAnhSanPhams.Select(h => h.Url).FirstOrDefault() ?? "/images/default-product.png"
            })
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    // ==========================================
    // 2. QUẢN LÝ KHÁCH HÀNG (TÌM KIẾM & THÊM NHANH)
    // ==========================================

    [HttpGet("customers/search")]
    public async Task<IActionResult> SearchCustomer([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Ok(new List<object>());

        var customers = await _dbContext.KhachHangs
            .Where(k => k.SoDienThoai.Contains(keyword) || k.Ten.Contains(keyword))
            .Select(k => new { k.KhachHangID, k.Ten, k.SoDienThoai })
            .Take(10) // Gợi ý tối đa 10 người để giao diện không bị giật
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    [HttpPost("customers/quick-add")]
    public async Task<IActionResult> QuickAddCustomer([FromBody] PosQuickAddCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var exists = await _dbContext.KhachHangs.AnyAsync(k => k.SoDienThoai == request.SoDienThoai, cancellationToken);
        if (exists) return BadRequest(new { message = "Số điện thoại này đã tồn tại trong hệ thống." });

        var newId = "KH" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        var customer = new KhachHang
        {
            KhachHangID = newId,
            Ten = request.Ten,
            SoDienThoai = request.SoDienThoai,
            Email = $"khach_{newId}@store.local", // Fake email để vượt qua validate nếu có
            GioiTinh = Enums.GioiTinh.Khac,
            DiaChi = "Mua trực tiếp tại cửa hàng",
            DiemTichLuy = 0,
            TaiKhoanID = "TK_POS_SYSTEM"
        };

        _dbContext.KhachHangs.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Thêm khách hàng thành công", khachHangID = newId, ten = customer.Ten, soDienThoai = customer.SoDienThoai });
    }



    // ==========================================
    // 3. XỬ LÝ THANH TOÁN GIAO DỊCH 
    // ==========================================
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckoutPOS([FromBody] PosCheckoutRequest request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest(new { message = "Giỏ hàng trống." });

        // Khởi tạo client PayOS bốc từ Service Container của bạn
        var payOSClient = HttpContext.RequestServices.GetService(typeof(PayOSClient)) as PayOSClient;

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            string finalKhachHangId = string.IsNullOrWhiteSpace(request.KhachHangID) ? "KH000000" : request.KhachHangID;

            // KIỂM TRA TÀI KHOẢN HỆ THỐNG
            var posAccount = await _dbContext.TaiKhoans.FindAsync(new object[] { "TK_POS_SYSTEM" }, cancellationToken);
            if (posAccount == null)
            {
                posAccount = new TaiKhoan
                {
                    TaiKhoanID = "TK_POS_SYSTEM",
                    Email = "pos_system@winterfashion.local",
                    MatKhau = "PosSystem123@",
                    TrangThai = "Hoạt động",
                    VaiTroID = "R03"
                };
                _dbContext.TaiKhoans.Add(posAccount);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // KIỂM TRA KHÁCH VÃNG LAI MẶC ĐỊNH
            var walkIn = await _dbContext.KhachHangs.FindAsync(new object[] { "KH000000" }, cancellationToken);
            if (walkIn == null)
            {
                _dbContext.KhachHangs.Add(new KhachHang
                {
                    KhachHangID = "KH000000",
                    Ten = "Khách vãng lai",
                    SoDienThoai = "0000000000",
                    Email = "khachle@winterfashion.local",
                    GioiTinh = Enums.GioiTinh.Khac,
                    DiaChi = "Mua trực tiếp tại quầy",
                    DiemTichLuy = 0,
                    TaiKhoanID = "TK_POS_SYSTEM"
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // KIỂM TRA ĐỊA CHỈ HỆ THỐNG
            var posAddress = await _dbContext.DiaChis.FindAsync(new object[] { "DC_POS_SYSTEM" }, cancellationToken);
            if (posAddress == null)
            {
                _dbContext.DiaChis.Add(new DiaChi
                {
                    DiaChiID = "DC_POS_SYSTEM",
                    KhachHangID = "KH000000",
                    TenNguoiNhan = "Khách mua tại quầy",
                    SoDienThoaiNhan = "0000000000",
                    TinhThanh = "Hà Nội",
                    QuanHuyen = "Sơn Tây",
                    PhuongXa = "Tại quầy||Tại quầy",
                    LaMacDinh = true
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var hoaDonId = "POS" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            decimal tongTienHang = 0;
            decimal tongTienVAT = 0;
            var chiTietList = new List<HoaDonChiTiet>();

            foreach (var item in request.Items)
            {
                var ctsp = await _dbContext.ChiTietSanPhams
                    .Include(c => c.SanPham)
                    .FirstOrDefaultAsync(c => c.ChiTietSanPhamID == item.ChiTietSanPhamID, cancellationToken);

                if (ctsp == null) return BadRequest(new { message = $"Không tìm thấy sản phẩm mã {item.ChiTietSanPhamID}" });
                if (ctsp.SoLuongTonKho < item.SoLuongMua) return BadRequest(new { message = $"Sản phẩm {ctsp.SanPham.Ten} không đủ tồn kho (Còn {ctsp.SoLuongTonKho})." });

                ctsp.SoLuongTonKho -= item.SoLuongMua;

                decimal mucVAT = ctsp.SanPham.MucVAT;
                decimal donGia = ctsp.GiaNiemYet;
                decimal tienVatItem = (donGia * mucVAT / 100) * item.SoLuongMua;

                tongTienHang += donGia * item.SoLuongMua;
                tongTienVAT += tienVatItem;

                chiTietList.Add(new HoaDonChiTiet
                {
                    HoaDonChiTietID = Guid.NewGuid().ToString(),
                    HoaDonID = hoaDonId,
                    ChiTietSanPhamID = ctsp.ChiTietSanPhamID,
                    SoLuong = item.SoLuongMua,
                    DonGia = donGia,
                    MucVAT = mucVAT,
                    TienVAT = tienVatItem
                });
            }

            decimal tongTienGiamGia = 0;
            if (!string.IsNullOrWhiteSpace(request.KhuyenMaiID))
            {
                var km = await _dbContext.KhuyenMais.FindAsync(new object[] { request.KhuyenMaiID }, cancellationToken);
                if (km != null && km.TrangThai == Enums.TrangThaiHoatDong.HoatDong && km.SoLuongDaDung < km.SoLuongToiDa)
                {
                    if (tongTienHang >= km.GiaTriToiThieuApDung)
                    {
                        if (km.LoaiGiamGia == Enums.LoaiGiamGia.TruThangTien)
                        {
                            tongTienGiamGia = km.GiaTriGiam;
                        }
                        else
                        {
                            decimal giamTheoPhanTram = tongTienHang * (km.GiaTriGiam / 100);
                            tongTienGiamGia = giamTheoPhanTram > km.GiamToiDa ? km.GiamToiDa : giamTheoPhanTram;
                        }
                        km.SoLuongDaDung += 1;
                    }
                }
            }

            decimal thanhTienCuoiCung = tongTienHang + tongTienVAT - tongTienGiamGia;
            if (thanhTienCuoiCung < 0) thanhTienCuoiCung = 0;

            var finalNhanVienId = await _dbContext.Set<NhanVien>().Select(nv => nv.NhanVienID).FirstOrDefaultAsync(cancellationToken) ?? "NV_DEFAULT";

            var hoaDon = new HoaDon
            {
                HoaDonID = hoaDonId,
                TongTienVAT = (double)tongTienVAT,
                TongTienGiamGia = (double)tongTienGiamGia,
                ThanhTien = (double)thanhTienCuoiCung,
                LoaiGiaoDich = Enums.LoaiGiaoDich.PosTaiQuay,
                // Chú ý: Nếu là QR Web thì tạm để trạng thái là Chờ duyệt (0) cho tới khi có Webhook báo tiền nổ thành công
                TrangThai = request.KieuThanhToan == 1 ? Enums.TrangThaiHoaDon.ChoDuyet : Enums.TrangThaiHoaDon.HoanThanh,
                GhiChu = request.GhiChu ?? "Bán hàng tại quầy POS",
                KhachHangID = finalKhachHangId,
                DiaChiID = string.IsNullOrWhiteSpace(request.DiaChiID) ? "DC_POS_SYSTEM" : request.DiaChiID,
                NhanVienID = finalNhanVienId,
                KhuyenMaiID = request.KhuyenMaiID,
                NgayTao = DateTime.Now
            };

            var activePhuongThucId = await _dbContext.Set<PhuongThucThanhToan>().Select(p => p.PhuongThucThanhToanID).FirstOrDefaultAsync(cancellationToken) ?? "PT_DEFAULT";

            // Sinh mã OrderCode dạng số nguyên độc nhất cho PayOS
            long payOsOrderCode = long.Parse(DateTime.Now.ToString("ddHHmmss"));
            string finalMaThamChieu = request.KieuThanhToan == 1 ? payOsOrderCode.ToString() : (request.MaThamChieu ?? string.Empty);

            var cttt = new ChiTietThanhToan
            {
                ChiTietThanhToanID = Guid.NewGuid().ToString(),
                HoaDonID = hoaDonId,
                Ten = request.KieuThanhToan == 0 ? "Tiền mặt" : "Chuyển khoản QR",
                SoTien = thanhTienCuoiCung,
                MaThamChieu = finalMaThamChieu,
                // Nếu trả tiền mặt thì Thành công liền, nếu là QR Code thì tạm thời để là Chờ thanh toán (0)
                TrangThai = Enums.TrangThaiThanhToan.ThatBai,
                PhuongThucThanhToanID = activePhuongThucId
            };

            _dbContext.HoaDons.Add(hoaDon);
            _dbContext.HoaDonChiTiets.AddRange(chiTietList);
            _dbContext.Set<ChiTietThanhToan>().Add(cttt);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 🔥 ĐOẠN ĐẶC BIỆT: GỌI SANG PAYOS ĐỂ LẤY LINK THANH TOÁN QR NẾU KHÁCH CHỌN PHƯƠNG THỨC QR
            string payOsCheckoutUrl = "";
            if (request.KieuThanhToan == 1 && payOSClient != null)
            {
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = payOsOrderCode,
                    Amount = (int)thanhTienCuoiCung,
                    Description = $"WINTERWEB {payOsOrderCode}",
                    CancelUrl = "https://localhost:7220/Home/Cart",
                    ReturnUrl = "https://localhost:7220/Home/Success"
                };

                var paymentLink = await payOSClient.PaymentRequests.CreateAsync(paymentRequest);
                payOsCheckoutUrl = paymentLink.CheckoutUrl;
            }

            await transaction.CommitAsync(cancellationToken);

            // Trả thêm thuộc tính kieuThanhToan và checkoutUrl ra ngoài Frontend
            return Ok(new { 
                message = "Thành công!", 
                hoaDonID = hoaDon.HoaDonID, 
                thoiGian = hoaDon.NgayTao,
                kieuThanhToan = request.KieuThanhToan,
                checkoutUrl = payOsCheckoutUrl,
                orderCode = finalMaThamChieu
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return StatusCode(500, new { message = "Lỗi xử lý giao dịch đơn hàng.", error = ex.Message });
        }
    }
}

#region DTOs phục vụ luồng POS (Nên đặt cùng file để dễ quản lý hoặc chuyển sang thư mục Models/DTOs)

public class PosProductDto
{
    public string ChiTietSanPhamID { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal GiaBan { get; set; }
    public int MucVAT { get; set; }
    public string KichCo { get; set; } = string.Empty;
    public string MauSac { get; set; } = string.Empty;
    public int SoLuongTonKho { get; set; }
    public string? HinhAnhUrl { get; set; }
}

public class PosQuickAddCustomerRequest
{
    [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
    public string Ten { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại VN không hợp lệ")]
    public string SoDienThoai { get; set; } = string.Empty;
}

public class PosCheckoutRequest
{
    public string? KhachHangID { get; set; }
    public string? DiaChiID { get; set; }
    public string? KhuyenMaiID { get; set; }
    public string? GhiChu { get; set; }
    public int KieuThanhToan { get; set; } // 0: Tiền mặt, 1: QR Code
    public string? MaThamChieu { get; set; } // Mã GD ngân hàng nếu quét QR

    [Required]
    public List<PosCheckoutItemDto> Items { get; set; } = new();
}

public class PosCheckoutItemDto
{
    [Required]
    public string ChiTietSanPhamID { get; set; } = string.Empty;

    [Required]
    public int SoLuongMua { get; set; }
}

#endregion