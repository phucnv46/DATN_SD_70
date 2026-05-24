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
        var now = DateTime.Now;
        var globalPromo = await _dbContext.KhuyenMais
            .Where(k => k.MaCode == null
                        && k.TrangThai == Enums.TrangThaiHoatDong.HoatDong
                        && k.NgayApDung <= now && k.NgayKetThuc >= now
                        && k.SoLuongDaDung < k.SoLuongToiDa
                        && !k.KhuyenMaiSanPhams.Any())
            .OrderByDescending(k => k.GiaTriGiam)
            .FirstOrDefaultAsync(cancellationToken);

        var products = await _dbContext.ChiTietSanPhams
            .Include(ct => ct.SanPham)
                .ThenInclude(sp => sp.HinhAnhSanPhams)
            .Include(ct => ct.KichCo)
            .Include(ct => ct.Mau)
            .Where(ct => ct.SoLuongTonKho > 0)
            .Select(ct => new
            {
                ct.ChiTietSanPhamID,
                ct.SanPham.Ten,
                ct.SKU,
                GiaNiemYet = ct.GiaNiemYet,
                ct.SanPham.MucVAT,
                KichCo = ct.KichCo != null ? ct.KichCo.Ten : "N/A",
                MauSac = ct.Mau != null ? ct.Mau.Ten : "N/A",
                ct.SoLuongTonKho,
                HinhAnhUrl = ct.SanPham.HinhAnhSanPhams.Select(h => h.Url).FirstOrDefault() ?? "/images/default-product.png",
                SanPhamID = ct.SanPhamID,
                DanhMucID = ct.SanPham.DanhMucID
            })
            .ToListAsync(cancellationToken);

        var result = products.Select(p =>
        {
            decimal giaGoc = p.GiaNiemYet;
            decimal giaSauGiam = giaGoc;
            string? badgeText = null;
            string? loaiGiam = null;

            if (globalPromo != null)
            {
                if (globalPromo.LoaiGiamGia == Enums.LoaiGiamGia.TruThangTien)
                {
                    giaSauGiam = giaGoc - globalPromo.GiaTriGiam;
                    if (giaSauGiam < 0) giaSauGiam = 0;
                    badgeText = $"-{globalPromo.GiaTriGiam:N0}đ";
                    loaiGiam = "money";
                }
                else
                {
                    decimal giam = giaGoc * (globalPromo.GiaTriGiam / 100);
                    if (globalPromo.GiamToiDa > 0 && giam > globalPromo.GiamToiDa)
                        giam = globalPromo.GiamToiDa;
                    giaSauGiam = giaGoc - giam;
                    if (giaSauGiam < 0) giaSauGiam = 0;
                    badgeText = $"-{globalPromo.GiaTriGiam}%";
                    loaiGiam = "percent";
                }
            }

            return new PosProductDto
            {
                ChiTietSanPhamID = p.ChiTietSanPhamID,
                TenSanPham = p.Ten,
                SKU = p.SKU,
                GiaGoc = giaGoc,
                GiaSauGiam = giaSauGiam,
                MucVAT = p.MucVAT,
                KichCo = p.KichCo,
                MauSac = p.MauSac,
                SoLuongTonKho = p.SoLuongTonKho,
                HinhAnhUrl = p.HinhAnhUrl,
                BadgeText = badgeText,
                LoaiGiam = loaiGiam,
                SanPhamID = p.SanPhamID ?? string.Empty,
                DanhMucID = p.DanhMucID
            };
        }).ToList();

        return Ok(result);
    }

    // ==========================================
    // 2. QUẢN LÝ KHÁCH HÀNG
    // ==========================================

    [HttpGet("customers/search")]
    public async Task<IActionResult> SearchCustomer([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return Ok(new List<object>());
        var customers = await _dbContext.KhachHangs
            .Where(k => k.SoDienThoai.Contains(keyword) || k.Ten.Contains(keyword))
            .Select(k => new { k.KhachHangID, k.Ten, k.SoDienThoai })
            .Take(10)
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
            Email = $"khach_{newId}@store.local",
            GioiTinh = Enums.GioiTinh.Khac,
            DiaChi = "Mua trực tiếp tại cửa hàng",
            DiemTichLuy = 0,
            TaiKhoanID = "TK_POS_SYSTEM"
        };
        _dbContext.KhachHangs.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Thêm khách hàng thành công", khachHangID = newId, ten = customer.Ten, soDienThoai = customer.SoDienThoai });
    }

    [HttpGet("check-stock/{chiTietSanPhamID}")]
    public async Task<IActionResult> CheckStock(string chiTietSanPhamID)
    {
        var item = await _dbContext.ChiTietSanPhams
            .Where(ct => ct.ChiTietSanPhamID == chiTietSanPhamID)
            .Select(ct => new { ct.SoLuongTonKho })
            .FirstOrDefaultAsync();
        if (item == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });
        return Ok(new { soLuongTonKho = item.SoLuongTonKho });
    }

    // ==========================================
    // 3. VOUCHER LOẠI 2 - LẤY DANH SÁCH KHẢ DỤNG
    // ==========================================

    [HttpGet("vouchers/available")]
    public async Task<IActionResult> GetAvailableVouchers([FromQuery] List<string> productIds, CancellationToken cancellationToken)
    {
        if (productIds == null || !productIds.Any()) return Ok(new List<object>());

        var now = DateTime.Now;
        var vouchers = await _dbContext.KhuyenMais
            .Include(k => k.KhuyenMaiSanPhams)
            .Where(k => k.MaCode != null
                        && k.TrangThai == Enums.TrangThaiHoatDong.HoatDong
                        && k.NgayApDung <= now && k.NgayKetThuc >= now
                        && k.SoLuongDaDung < k.SoLuongToiDa)
            .ToListAsync(cancellationToken);

        var productIdsInCart = await _dbContext.ChiTietSanPhams
            .Where(ct => productIds.Contains(ct.ChiTietSanPhamID))
            .Select(ct => new { ct.ChiTietSanPhamID, ct.SanPhamID, ct.SanPham.DanhMucID })
            .ToListAsync(cancellationToken);

        var cartProductIds = productIdsInCart.Select(p => p.SanPhamID).Distinct().ToList();
        var cartCategoryIds = productIdsInCart.Select(p => p.DanhMucID).Distinct().ToList();

        var availableVouchers = new List<object>();

        foreach (var voucher in vouchers)
        {
            bool isApplicable = false;
            List<string>? applicableSanPhamIds = null;
            string? applicableDanhMucId = null;

            if (voucher.KhuyenMaiSanPhams.Any())
            {
                applicableSanPhamIds = voucher.KhuyenMaiSanPhams.Select(kms => kms.SanPhamID).ToList();
                isApplicable = cartProductIds.Any(pid => applicableSanPhamIds.Contains(pid));
            }
            else if (!string.IsNullOrEmpty(voucher.DanhMucID))
            {
                applicableDanhMucId = voucher.DanhMucID;
                isApplicable = cartCategoryIds.Contains(voucher.DanhMucID);
            }
            else
            {
                isApplicable = true;
            }

            if (isApplicable)
            {
                availableVouchers.Add(new
                {
                    khuyenMaiId = voucher.KhuyenMaiID,
                    maCode = voucher.MaCode,
                    ten = voucher.Ten,
                    loaiGiamGia = (int)voucher.LoaiGiamGia,
                    giaTriGiam = voucher.GiaTriGiam,
                    giamToiDa = voucher.GiamToiDa,
                    giaTriToiThieuApDung = voucher.GiaTriToiThieuApDung,
                    moTa = voucher.MoTa ?? "",
                    applicableSanPhamIds = applicableSanPhamIds,
                    applicableDanhMucId = applicableDanhMucId
                });
            }
        }

        return Ok(availableVouchers);
    }

    // Hủy đơn hàng QR
    [HttpPost("checkout/{hoaDonId}/cancel")]
    public async Task<IActionResult> CancelCheckout(string hoaDonId, CancellationToken cancellationToken)
    {
        var hoaDon = await _dbContext.HoaDons
            .Include(h => h.HoaDonChiTiets)
            .FirstOrDefaultAsync(h => h.HoaDonID == hoaDonId, cancellationToken);

        if (hoaDon == null) return NotFound(new { message = "Không tìm thấy hóa đơn." });

        if (hoaDon.TrangThai != Enums.TrangThaiHoaDon.ChoDuyet)
            return BadRequest(new { message = "Chỉ có thể hủy hóa đơn đang chờ thanh toán QR." });

        // Hoàn trả tồn kho
        foreach (var chiTiet in hoaDon.HoaDonChiTiets)
        {
            var ctsp = await _dbContext.ChiTietSanPhams.FindAsync(new object[] { chiTiet.ChiTietSanPhamID }, cancellationToken);
            if (ctsp != null)
                ctsp.SoLuongTonKho += chiTiet.SoLuong;
        }

        // Hoàn trả lượt dùng voucher toàn sàn (nếu có)
        if (!string.IsNullOrEmpty(hoaDon.KhuyenMaiID))
        {
            var km = await _dbContext.KhuyenMais.FindAsync(new object[] { hoaDon.KhuyenMaiID }, cancellationToken);
            if (km != null && km.SoLuongDaDung > 0)
                km.SoLuongDaDung -= 1;
        }

        hoaDon.TrangThai = Enums.TrangThaiHoaDon.DaHuy;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Đã hủy đơn hàng thành công." });
    }

    // ==========================================
    // 4. XỬ LÝ THANH TOÁN (đã sửa logic giảm giá + VAT)
    // ==========================================

    [HttpPost("checkout")]
    public async Task<IActionResult> CheckoutPOS([FromBody] PosCheckoutRequest request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest(new { message = "Giỏ hàng trống." });

        var payOSClient = HttpContext.RequestServices.GetService(typeof(PayOSClient)) as PayOSClient;

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            string finalKhachHangId = string.IsNullOrWhiteSpace(request.KhachHangID) ? "KH000000" : request.KhachHangID;

            // Hệ thống POS account/khách lẻ/địa chỉ (giữ nguyên)
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

            // 1️⃣ Khuyến mãi toàn sàn (Loại 1)
            var now = DateTime.Now;
            var globalPromo = await _dbContext.KhuyenMais
                .Where(k => k.MaCode == null
                            && k.TrangThai == Enums.TrangThaiHoatDong.HoatDong
                            && k.NgayApDung <= now && k.NgayKetThuc >= now
                            && k.SoLuongDaDung < k.SoLuongToiDa
                            && !k.KhuyenMaiSanPhams.Any())
                .OrderByDescending(k => k.GiaTriGiam)
                .FirstOrDefaultAsync(cancellationToken);

            var hoaDonId = "POS" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            decimal tongTienHang = 0;   // sẽ tính lại sau khi có giá cuối cùng
            decimal tongTienVAT = 0;
            var chiTietList = new List<HoaDonChiTiet>();
            var cartItems = new List<(string ChiTietSanPhamID, int SoLuong, decimal DonGiaSauGlobal, string? DanhMucID, string SanPhamID, decimal MucVAT)>();

            // Bước 1: Tính giá sau KM toàn sàn và lưu vào cartItems, chưa lưu vào chiTietList vội
            foreach (var item in request.Items)
            {
                var ctsp = await _dbContext.ChiTietSanPhams
                    .Include(c => c.SanPham)
                    .FirstOrDefaultAsync(c => c.ChiTietSanPhamID == item.ChiTietSanPhamID, cancellationToken);

                if (ctsp == null) return BadRequest(new { message = $"Không tìm thấy sản phẩm mã {item.ChiTietSanPhamID}" });
                if (ctsp.SoLuongTonKho < item.SoLuongMua) return BadRequest(new { message = $"Sản phẩm {ctsp.SanPham.Ten} không đủ tồn kho (Còn {ctsp.SoLuongTonKho})." });

                ctsp.SoLuongTonKho -= item.SoLuongMua;

                decimal donGiaSauGlobal = ctsp.GiaNiemYet;
                if (globalPromo != null)
                {
                    if (globalPromo.LoaiGiamGia == Enums.LoaiGiamGia.TruThangTien)
                    {
                        donGiaSauGlobal = ctsp.GiaNiemYet - globalPromo.GiaTriGiam;
                    }
                    else
                    {
                        decimal giam = ctsp.GiaNiemYet * (globalPromo.GiaTriGiam / 100);
                        if (globalPromo.GiamToiDa > 0 && giam > globalPromo.GiamToiDa)
                            giam = globalPromo.GiamToiDa;
                        donGiaSauGlobal = ctsp.GiaNiemYet - giam;
                    }
                    if (donGiaSauGlobal < 0) donGiaSauGlobal = 0;
                }

                cartItems.Add((ctsp.ChiTietSanPhamID, item.SoLuongMua, donGiaSauGlobal, ctsp.SanPham.DanhMucID, ctsp.SanPhamID, ctsp.SanPham.MucVAT));
            }

            // 2️⃣ Xử lý voucher Loại 2 & tính giảm giá cho từng sản phẩm
            decimal tongTienGiamGiaVoucher = 0;
            var selectedVoucherIds = request.VoucherIds ?? new List<string>();
            var discountPerProduct = new Dictionary<string, decimal>(); // key: ChiTietSanPhamID, value: tổng tiền giảm (cho cả dòng)

            if (selectedVoucherIds.Any())
            {
                var vouchers = await _dbContext.KhuyenMais
                    .Include(k => k.KhuyenMaiSanPhams)
                    .Where(k => selectedVoucherIds.Contains(k.KhuyenMaiID)
                                && k.MaCode != null
                                && k.TrangThai == Enums.TrangThaiHoatDong.HoatDong
                                && k.NgayApDung <= now && k.NgayKetThuc >= now
                                && k.SoLuongDaDung < k.SoLuongToiDa)
                    .ToListAsync(cancellationToken);

                if (vouchers.Any())
                {
                    var bestVoucherPerProduct = new Dictionary<string, (string VoucherId, decimal TienGiam)>();

                    foreach (var cartItem in cartItems)
                    {
                        decimal bestDiscount = 0;
                        string? bestVoucherId = null;

                        foreach (var voucher in vouchers)
                        {
                            bool isApplicable = true;
                            if (voucher.KhuyenMaiSanPhams.Any())
                            {
                                isApplicable = voucher.KhuyenMaiSanPhams.Any(kms => kms.SanPhamID == cartItem.SanPhamID);
                            }
                            else if (!string.IsNullOrEmpty(voucher.DanhMucID))
                            {
                                isApplicable = cartItem.DanhMucID == voucher.DanhMucID;
                            }

                            if (!isApplicable) continue;

                            decimal discountForItem = 0;
                            decimal itemTotal = cartItem.DonGiaSauGlobal * cartItem.SoLuong;

                            if (voucher.LoaiGiamGia == Enums.LoaiGiamGia.TruThangTien)
                            {
                                discountForItem = voucher.GiaTriGiam * cartItem.SoLuong;
                                if (discountForItem > itemTotal) discountForItem = itemTotal;
                            }
                            else
                            {
                                decimal giam = itemTotal * (voucher.GiaTriGiam / 100);
                                if (voucher.GiamToiDa > 0 && giam > voucher.GiamToiDa)
                                    giam = voucher.GiamToiDa;
                                discountForItem = giam;
                            }

                            if (discountForItem > bestDiscount)
                            {
                                bestDiscount = discountForItem;
                                bestVoucherId = voucher.KhuyenMaiID;
                            }
                        }

                        if (bestVoucherId != null)
                        {
                            bestVoucherPerProduct[cartItem.ChiTietSanPhamID] = (bestVoucherId, bestDiscount);
                        }
                    }

                    // Gom nhóm giảm giá theo voucher để kiểm tra điều kiện đơn tối thiểu
                    var discountByVoucher = new Dictionary<string, (decimal Discount, decimal MinOrder, int Remaining)>();

                    foreach (var kvp in bestVoucherPerProduct)
                    {
                        var voucherId = kvp.Value.VoucherId;
                        var discount = kvp.Value.TienGiam;

                        if (!discountByVoucher.ContainsKey(voucherId))
                        {
                            var v = vouchers.First(x => x.KhuyenMaiID == voucherId);
                            discountByVoucher[voucherId] = (0, v.GiaTriToiThieuApDung, v.SoLuongToiDa - v.SoLuongDaDung);
                        }
                        var entry = discountByVoucher[voucherId];
                        discountByVoucher[voucherId] = (entry.Discount + discount, entry.MinOrder, entry.Remaining);
                    }

                    // Loại voucher không đạt điều kiện giá trị đơn hàng tối thiểu
                    foreach (var voucherId in discountByVoucher.Keys.ToList())
                    {
                        var (totalDiscount, minOrder, _) = discountByVoucher[voucherId];
                        decimal orderValueForVoucher = 0;
                        var voucher = vouchers.First(v => v.KhuyenMaiID == voucherId);

                        foreach (var cartItem in cartItems)
                        {
                            bool inScope = !voucher.KhuyenMaiSanPhams.Any() ||
                                           voucher.KhuyenMaiSanPhams.Any(kms => kms.SanPhamID == cartItem.SanPhamID);
                            if (!inScope && !string.IsNullOrEmpty(voucher.DanhMucID))
                                inScope = cartItem.DanhMucID == voucher.DanhMucID;
                            if (inScope)
                                orderValueForVoucher += cartItem.DonGiaSauGlobal * cartItem.SoLuong;
                        }

                        if (orderValueForVoucher < minOrder)
                        {
                            discountByVoucher.Remove(voucherId);
                            var productsToRemove = bestVoucherPerProduct.Where(kvp => kvp.Value.VoucherId == voucherId).Select(kvp => kvp.Key).ToList();
                            foreach (var pid in productsToRemove) bestVoucherPerProduct.Remove(pid);
                        }
                    }

                    // Tính tổng giảm giá & lưu discountPerProduct
                    foreach (var kvp in bestVoucherPerProduct)
                    {
                        var chiTietId = kvp.Key;
                        var discount = kvp.Value.TienGiam;
                        discountPerProduct[chiTietId] = discount;
                        tongTienGiamGiaVoucher += discount;
                    }

                    // Tăng lượt dùng
                    foreach (var voucherId in discountByVoucher.Keys)
                    {
                        var voucher = vouchers.First(v => v.KhuyenMaiID == voucherId);
                        voucher.SoLuongDaDung += 1;
                    }
                }
            }

            // 3️⃣ Xây dựng lại chiTietList với giá cuối cùng và VAT tương ứng
            chiTietList.Clear();
            tongTienHang = 0;
            tongTienVAT = 0;

            foreach (var cartItem in cartItems)
            {
                decimal donGiaSauGlobal = cartItem.DonGiaSauGlobal;
                decimal giamVoucher = discountPerProduct.ContainsKey(cartItem.ChiTietSanPhamID)
                                    ? discountPerProduct[cartItem.ChiTietSanPhamID] / cartItem.SoLuong
                                    : 0;

                decimal donGiaCuoi = donGiaSauGlobal - giamVoucher;
                if (donGiaCuoi < 0) donGiaCuoi = 0;

                decimal mucVAT = cartItem.MucVAT;
                decimal tienVatItem = (donGiaCuoi * mucVAT / 100) * cartItem.SoLuong;

                tongTienHang += donGiaCuoi * cartItem.SoLuong;
                tongTienVAT += tienVatItem;

                chiTietList.Add(new HoaDonChiTiet
                {
                    HoaDonChiTietID = Guid.NewGuid().ToString(),
                    HoaDonID = hoaDonId,
                    ChiTietSanPhamID = cartItem.ChiTietSanPhamID,
                    SoLuong = cartItem.SoLuong,
                    DonGia = donGiaCuoi,
                    MucVAT = mucVAT,
                    TienVAT = tienVatItem
                });
            }

            decimal thanhTienCuoiCung = tongTienHang + tongTienVAT;
            if (thanhTienCuoiCung < 0) thanhTienCuoiCung = 0;

            var finalNhanVienId = await _dbContext.Set<NhanVien>().Select(nv => nv.NhanVienID).FirstOrDefaultAsync(cancellationToken) ?? "NV_DEFAULT";

            var hoaDon = new HoaDon
            {
                HoaDonID = hoaDonId,
                TongTienVAT = (double)tongTienVAT,
                TongTienGiamGia = (double)tongTienGiamGiaVoucher,
                ThanhTien = (double)thanhTienCuoiCung,
                LoaiGiaoDich = Enums.LoaiGiaoDich.PosTaiQuay,
                TrangThai = request.KieuThanhToan == 1 ? Enums.TrangThaiHoaDon.ChoDuyet
                            : (request.KieuThanhToan == 2 ? Enums.TrangThaiHoaDon.HoanThanh
                            : Enums.TrangThaiHoaDon.HoanThanh),
                GhiChu = request.GhiChu ?? "Bán hàng tại quầy POS",
                KhachHangID = finalKhachHangId,
                DiaChiID = string.IsNullOrWhiteSpace(request.DiaChiID) ? "DC_POS_SYSTEM" : request.DiaChiID,
                NhanVienID = finalNhanVienId,
                KhuyenMaiID = globalPromo?.KhuyenMaiID,
                NgayTao = DateTime.Now
            };

            var activePhuongThucId = await _dbContext.Set<PhuongThucThanhToan>().Select(p => p.PhuongThucThanhToanID).FirstOrDefaultAsync(cancellationToken) ?? "PT_DEFAULT";

            long payOsOrderCode = GenerateUniquePayOSOrderCode();
            string finalMaThamChieu = request.KieuThanhToan == 1 ? payOsOrderCode.ToString() : (request.MaThamChieu ?? string.Empty);

            var cttt = new ChiTietThanhToan
            {
                ChiTietThanhToanID = Guid.NewGuid().ToString(),
                HoaDonID = hoaDonId,
                Ten = request.KieuThanhToan == 0 ? "Tiền mặt" : "Chuyển khoản QR",
                SoTien = thanhTienCuoiCung,
                MaThamChieu = finalMaThamChieu,
                TrangThai = Enums.TrangThaiThanhToan.ThatBai,
                PhuongThucThanhToanID = activePhuongThucId
            };

            _dbContext.HoaDons.Add(hoaDon);
            _dbContext.HoaDonChiTiets.AddRange(chiTietList);
            _dbContext.Set<ChiTietThanhToan>().Add(cttt);

            if (globalPromo != null)
                globalPromo.SoLuongDaDung += 1;

            await _dbContext.SaveChangesAsync(cancellationToken);

            string payOsCheckoutUrl = "";
            if (request.KieuThanhToan == 1 && payOSClient != null)
            {
                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = payOsOrderCode,
                    Amount = (int)thanhTienCuoiCung,
                    Description = $"POS{payOsOrderCode}".Length > 25
                        ? $"POS{payOsOrderCode}".Substring(0, 25)
                        : $"POS{payOsOrderCode}",
                    CancelUrl = "https://localhost:7220/Home/Cart",
                    ReturnUrl = "https://localhost:7220/Home/Success"
                };

                var paymentLink = await payOSClient.PaymentRequests.CreateAsync(paymentRequest);
                payOsCheckoutUrl = paymentLink.CheckoutUrl;
            }

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Thành công!",
                hoaDonID = hoaDon.HoaDonID,
                thoiGian = hoaDon.NgayTao,
                kieuThanhToan = request.KieuThanhToan,
                checkoutUrl = payOsCheckoutUrl,
                orderCode = finalMaThamChieu,
                tongTienHang = tongTienHang,      // tổng tiền hàng sau tất cả giảm giá
                tongTienVAT = tongTienVAT,        // tổng VAT thực tế
                tongGiamGia = tongTienGiamGiaVoucher
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return StatusCode(500, new { message = "Lỗi xử lý giao dịch đơn hàng.", error = ex.Message });
        }
    }

    private long GenerateUniquePayOSOrderCode()
    {
        var timestampPart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var randomPart = new Random().Next(100, 999).ToString();
        return long.Parse(timestampPart + randomPart);
    }
}

#region DTOs

public class PosProductDto
{
    public string ChiTietSanPhamID { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal GiaGoc { get; set; }
    public decimal GiaSauGiam { get; set; }
    public int MucVAT { get; set; }
    public string KichCo { get; set; } = string.Empty;
    public string MauSac { get; set; } = string.Empty;
    public int SoLuongTonKho { get; set; }
    public string? HinhAnhUrl { get; set; }
    public string? BadgeText { get; set; }
    public string? LoaiGiam { get; set; }
    public string SanPhamID { get; set; } = string.Empty;
    public string? DanhMucID { get; set; }
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
    public int KieuThanhToan { get; set; }
    public string? MaThamChieu { get; set; }
    public List<string>? VoucherIds { get; set; }

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