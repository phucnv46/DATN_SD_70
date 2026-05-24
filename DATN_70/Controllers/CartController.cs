using DATN_70.Data;
using DATN_70.Models.Cart;
using DATN_70.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CartController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CartController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken);
        if (account is null)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập." });
        }

        var cart = await EnsureCartAsync(account.TaiKhoanID, cancellationToken);
        var response = await BuildCartResponseAsync(cart.GioHangID, cancellationToken);
        return Ok(response);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var account = await GetCurrentAccountAsync(cancellationToken);
        if (account is null)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập." });
        }

        var cart = await EnsureCartAsync(account.TaiKhoanID, cancellationToken);
        var variant = await _dbContext.ChiTietSanPhams
            .Include(item => item.SanPham)
            .Include(item => item.Mau)
            .Include(item => item.KichCo)
            .FirstOrDefaultAsync(item => item.ChiTietSanPhamID == request.ChiTietSanPhamID, cancellationToken);

        if (variant is null)
        {
            return NotFound(new { message = "Không tìm thấy biến thể sản phẩm." });
        }

        var line = await _dbContext.ChiTietGioHangs
            .FirstOrDefaultAsync(item => item.GioHangID == cart.GioHangID && item.ChiTietSanPhamID == request.ChiTietSanPhamID, cancellationToken);

        var nextQuantity = (line?.SoLuong ?? 0) + request.SoLuong;
        if (nextQuantity > Math.Min(20, variant.SoLuongTonKho))
        {
            return BadRequest(new { message = $"Số lượng tối đa cho biến thể này là {Math.Min(20, variant.SoLuongTonKho)}." });
        }

        if (line is null)
        {
            var currentUnitPrice = await GetCurrentUnitPriceAsync(variant, cancellationToken);
            _dbContext.ChiTietGioHangs.Add(new ChiTietGioHang
            {
                ChiTietGioHangID = Guid.NewGuid().ToString(),
                GioHangID = cart.GioHangID,
                ChiTietSanPhamID = variant.ChiTietSanPhamID,
                SoLuong = request.SoLuong,
                TongTien = currentUnitPrice * request.SoLuong
            });
        }
        else
        {
            var currentUnitPrice = await GetCurrentUnitPriceAsync(variant, cancellationToken);
            line.SoLuong = nextQuantity;
            line.TongTien = currentUnitPrice * nextQuantity;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await BuildCartResponseAsync(cart.GioHangID, cancellationToken));
    }

    [HttpPut("items/{productDetailId}")]
    public async Task<ActionResult<CartResponse>> UpdateItem(
        string productDetailId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var account = await GetCurrentAccountAsync(cancellationToken);
        if (account is null)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập." });
        }

        var cart = await EnsureCartAsync(account.TaiKhoanID, cancellationToken);
        var line = await _dbContext.ChiTietGioHangs
            .FirstOrDefaultAsync(item => item.GioHangID == cart.GioHangID && item.ChiTietSanPhamID == productDetailId, cancellationToken);

        if (line is null)
        {
            return NotFound(new { message = "Sản phẩm không có trong giỏ hàng." });
        }

        if (request.SoLuong <= 0)
        {
            _dbContext.ChiTietGioHangs.Remove(line);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(await BuildCartResponseAsync(cart.GioHangID, cancellationToken));
        }

        var variant = await _dbContext.ChiTietSanPhams.FirstOrDefaultAsync(item => item.ChiTietSanPhamID == productDetailId, cancellationToken);
        if (variant is null)
        {
            return NotFound(new { message = "Không tìm thấy biến thể sản phẩm." });
        }

        if (request.SoLuong > Math.Min(20, variant.SoLuongTonKho))
        {
            return BadRequest(new { message = $"Số lượng tối đa cho biến thể này là {Math.Min(20, variant.SoLuongTonKho)}." });
        }

        line.SoLuong = request.SoLuong;
        line.TongTien = await GetCurrentUnitPriceAsync(variant, cancellationToken) * request.SoLuong;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await BuildCartResponseAsync(cart.GioHangID, cancellationToken));
    }

    [HttpDelete("items/{productDetailId}")]
    public async Task<ActionResult<CartResponse>> RemoveItem(
        string productDetailId,
        CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken);
        if (account is null)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập." });
        }

        var cart = await EnsureCartAsync(account.TaiKhoanID, cancellationToken);
        var line = await _dbContext.ChiTietGioHangs
            .FirstOrDefaultAsync(item => item.GioHangID == cart.GioHangID && item.ChiTietSanPhamID == productDetailId, cancellationToken);

        if (line is not null)
        {
            _dbContext.ChiTietGioHangs.Remove(line);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(await BuildCartResponseAsync(cart.GioHangID, cancellationToken));
    }
    private async Task<KhuyenMai?> GetActiveGlobalPromoAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        return await _dbContext.KhuyenMais
            .FirstOrDefaultAsync(k =>
                k.MaCode == null &&
                k.TrangThai == Models.Enums.Enums.TrangThaiHoatDong.HoatDong &&
                k.NgayApDung <= now && k.NgayKetThuc >= now &&
                (k.SoLuongToiDa == 0 || k.SoLuongDaDung < k.SoLuongToiDa) &&
                !k.KhuyenMaiSanPhams.Any(),
            cancellationToken);
    }
    [HttpDelete]
    public async Task<ActionResult<CartResponse>> ClearCart(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken);
        if (account is null)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập." });
        }

        var cart = await EnsureCartAsync(account.TaiKhoanID, cancellationToken);
        var lines = await _dbContext.ChiTietGioHangs
            .Where(item => item.GioHangID == cart.GioHangID)
            .ToListAsync(cancellationToken);

        if (lines.Count > 0)
        {
            _dbContext.ChiTietGioHangs.RemoveRange(lines);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(await BuildCartResponseAsync(cart.GioHangID, cancellationToken));
    }

    private async Task<TaiKhoan?> GetCurrentAccountAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _dbContext.TaiKhoans.FirstOrDefaultAsync(item => item.TaiKhoanID == userId, cancellationToken);
    }

    private async Task<GioHang> EnsureCartAsync(string accountId, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.GioHangs.FirstOrDefaultAsync(item => item.TaiKhoanID == accountId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = new GioHang
        {
            GioHangID = Guid.NewGuid().ToString(),
            TaiKhoanID = accountId,
            NgayTao = DateTime.Now
        };

        _dbContext.GioHangs.Add(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return cart;
    }

    private async Task<CartResponse> BuildCartResponseAsync(string cartId, CancellationToken cancellationToken)
    {
        var items = await _dbContext.ChiTietGioHangs
            .AsNoTracking()
            .Include(i => i.ChiTietSanPham)
                .ThenInclude(i => i.SanPham)
            .Include(i => i.ChiTietSanPham)
                .ThenInclude(i => i.Mau)
            .Include(i => i.ChiTietSanPham)
                .ThenInclude(i => i.KichCo)
            .Where(i => i.GioHangID == cartId)
            .OrderBy(i => i.ChiTietSanPham.SanPham.Ten)
            .ThenBy(i => i.ChiTietSanPham.Mau.Ten)
            .ThenBy(i => i.ChiTietSanPham.KichCo.Ten)
            .ToListAsync(cancellationToken);

        // Lấy khuyến mãi toàn sàn
        var globalPromo = await GetActiveGlobalPromoAsync(cancellationToken);

        // Lấy khuyến mãi theo sản phẩm
        var discounts = await GetActiveDiscountsAsync(
            items.Select(i => i.ChiTietSanPham.SanPhamID).Distinct().ToList(),
            cancellationToken);

        return new CartResponse
        {
            Items = items.Select(item =>
            {
                var basePrice = item.ChiTietSanPham.GiaNiemYet;
                var vatRate = item.ChiTietSanPham.SanPham.MucVAT;

                // Ưu tiên khuyến mãi toàn sàn, nếu không có thì dùng khuyến mãi sản phẩm
                // Lấy khuyến mãi riêng nếu có
                var productPromo = discounts.GetValueOrDefault(item.ChiTietSanPham.SanPhamID);
                // Chọn khuyến mãi cho giá thấp nhất
                KhuyenMai? bestPromo = null;
                decimal bestPrice = basePrice;
                if (globalPromo != null)
                {
                    var globalPrice = ApplyDiscount(basePrice, globalPromo);
                    if (globalPrice < bestPrice) { bestPrice = globalPrice; bestPromo = globalPromo; }
                }
                if (productPromo != null)
                {
                    var productPrice = ApplyDiscount(basePrice, productPromo);
                    if (productPrice < bestPrice) { bestPrice = productPrice; bestPromo = productPromo; }
                }
                var finalPrice = bestPrice; // dùng cho DonGia
                

                return new CartItemResponse
                {
                    SanPhamID = item.ChiTietSanPham.SanPhamID,
                    ChiTietSanPhamID = item.ChiTietSanPhamID,
                    TenSanPham = item.ChiTietSanPham.SanPham.Ten,
                    PhanLoai = $"{item.ChiTietSanPham.Mau.Ten} / {item.ChiTietSanPham.KichCo.Ten.Replace("Size ", string.Empty)}",
                    SoLuong = item.SoLuong,
                    DonGia = finalPrice,          // giá sau khuyến mãi
                    GiaGoc = basePrice,           // giá gốc để hiển thị nếu cần
                    VatRate = vatRate,            // để tính VAT
                    TonKho = item.ChiTietSanPham.SoLuongTonKho
                };
            }).ToList()
        };
    }

    private async Task<decimal> GetCurrentUnitPriceAsync(ChiTietSanPham variant, CancellationToken cancellationToken)
    {
        var basePrice = variant.GiaNiemYet;
        decimal bestPrice = basePrice;

        // Kiểm tra khuyến mãi riêng
        var discounts = await GetActiveDiscountsAsync([variant.SanPhamID], cancellationToken);
        var productPromo = discounts.GetValueOrDefault(variant.SanPhamID);
        if (productPromo != null)
        {
            var productPrice = ApplyDiscount(basePrice, productPromo);
            if (productPrice < bestPrice) bestPrice = productPrice;
        }

        // Kiểm tra khuyến mãi toàn sàn
        var globalPromo = await GetActiveGlobalPromoAsync(cancellationToken);
        if (globalPromo != null)
        {
            var globalPrice = ApplyDiscount(basePrice, globalPromo);
            if (globalPrice < bestPrice) bestPrice = globalPrice;
        }

        return bestPrice;
    }

    // --- HÀM LẤY KHUYẾN MÃI ĐÃ ĐƯỢC CẬP NHẬT ĐỂ LẤY TOÀN BỘ OBJECT KHUYENMAI ---
    private async Task<Dictionary<string, KhuyenMai>> GetActiveDiscountsAsync(
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return new Dictionary<string, KhuyenMai>();
        }

        var now = DateTime.Now;
        var promos = await _dbContext.KhuyenMaiSanPhams
            .AsNoTracking()
            .Include(item => item.KhuyenMai)
            .Where(item => productIds.Contains(item.SanPhamID)
                && item.KhuyenMai.TrangThai == Models.Enums.Enums.TrangThaiHoatDong.HoatDong
                && item.KhuyenMai.NgayApDung <= now
                && item.KhuyenMai.NgayKetThuc >= now)
            .ToListAsync(cancellationToken);

        var dict = new Dictionary<string, KhuyenMai>();
        foreach (var p in promos)
        {
            // Lấy chương trình khuyến mãi đầu tiên khả dụng cho sản phẩm này
            if (!dict.ContainsKey(p.SanPhamID) && p.KhuyenMai != null)
            {
                dict[p.SanPhamID] = p.KhuyenMai;
            }
        }
        return dict;
    }

    // --- HÀM TÍNH TOÁN GIẢM GIÁ THÔNG MINH CHO CẢ 2 LOẠI (TRỪ TIỀN VÀ PHẦN TRĂM) ---
    private static decimal ApplyDiscount(decimal basePrice, KhuyenMai? promo)
    {
        // Nếu không có khuyến mãi hoặc giá trị giảm <= 0 thì giữ nguyên giá gốc
        if (promo == null || promo.GiaTriGiam <= 0)
        {
            return basePrice;
        }

        decimal tienGiam = 0;

        if ((int)promo.LoaiGiamGia == 1) // 1: Dạng giảm Phần trăm
        {
            tienGiam = basePrice * (promo.GiaTriGiam / 100m);
            // Áp dụng trần tối đa nếu có
            if (tienGiam > promo.GiamToiDa && promo.GiamToiDa > 0)
            {
                tienGiam = promo.GiamToiDa;
            }
        }
        else // 0: Dạng Trừ thẳng tiền
        {
            tienGiam = promo.GiaTriGiam;
        }

        decimal finalPrice = basePrice - tienGiam;

        // Chống âm tiền nếu số tiền giảm lớn hơn cả giá sản phẩm, sau đó làm tròn
        return finalPrice < 0 ? 0 : Math.Round(finalPrice, 0, MidpointRounding.AwayFromZero);
    }
    [HttpGet("available-coupons")]
    public async Task<IActionResult> GetAvailableCoupons(CancellationToken cancellationToken)
    {
        var account = await GetCurrentAccountAsync(cancellationToken);
        if (account is null)
            return Unauthorized(new { message = "Vui lòng đăng nhập." });

        var cart = await _dbContext.GioHangs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TaiKhoanID == account.TaiKhoanID, cancellationToken);

        if (cart is null)
            return Ok(new List<object>());

        // Lấy danh sách chi tiết sản phẩm trong giỏ hàng
        var cartItems = await _dbContext.ChiTietGioHangs
            .AsNoTracking()
            .Include(ci => ci.ChiTietSanPham)
                .ThenInclude(ct => ct.SanPham)
            .Where(ci => ci.GioHangID == cart.GioHangID)
            .Select(ci => new
            {
                ci.ChiTietSanPhamID,
                ci.ChiTietSanPham.SanPhamID,
                ci.ChiTietSanPham.SanPham.DanhMucID,
                ci.SoLuong,
                ci.ChiTietSanPham.GiaNiemYet
            })
            .ToListAsync(cancellationToken);

        if (!cartItems.Any())
            return Ok(new List<object>());

        // Lấy tất cả voucher loại 2 (có MaCode) đang hoạt động
        var now = DateTime.Now;
        var allVouchers = await _dbContext.KhuyenMais
            .AsNoTracking()
            .Where(k => k.MaCode != null
                        && k.TrangThai == Models.Enums.Enums.TrangThaiHoatDong.HoatDong
                        && k.NgayApDung <= now && k.NgayKetThuc >= now
                        && (k.SoLuongToiDa == 0 || k.SoLuongDaDung < k.SoLuongToiDa))
            .ToListAsync(cancellationToken);

        // Lọc voucher khả dụng: có phạm vi bao phủ ít nhất một sản phẩm trong giỏ
        var availableCoupons = new List<object>();
        foreach (var voucher in allVouchers)
        {
            bool isApplicable = false;

            // Voucher toàn sàn có mã (không ràng buộc sản phẩm)
            if (!voucher.KhuyenMaiSanPhams.Any() && string.IsNullOrEmpty(voucher.DanhMucID))
            {
                isApplicable = true;
            }
            // Voucher theo danh mục
            else if (!string.IsNullOrEmpty(voucher.DanhMucID))
            {
                isApplicable = cartItems.Any(ci => ci.DanhMucID == voucher.DanhMucID);
            }
            // Voucher theo sản phẩm cụ thể
            else if (voucher.KhuyenMaiSanPhams.Any())
            {
                var applicableProductIds = voucher.KhuyenMaiSanPhams.Select(ksp => ksp.SanPhamID).ToHashSet();
                isApplicable = cartItems.Any(ci => applicableProductIds.Contains(ci.SanPhamID));
            }

            if (isApplicable)
            {
                availableCoupons.Add(new
                {
                    maCode = voucher.MaCode,
                    ten = voucher.Ten,
                    moTa = voucher.MoTa ?? "",
                    loaiGiamGia = (int)voucher.LoaiGiamGia,
                    giaTriGiam = voucher.GiaTriGiam,
                    giamToiDa = voucher.GiamToiDa,
                    giaTriToiThieuApDung = voucher.GiaTriToiThieuApDung
                });
            }
        }

        return Ok(availableCoupons);
    }
}