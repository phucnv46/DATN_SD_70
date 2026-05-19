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
            .Include(item => item.ChiTietSanPham)
                .ThenInclude(item => item.SanPham)
            .Include(item => item.ChiTietSanPham)
                .ThenInclude(item => item.Mau)
            .Include(item => item.ChiTietSanPham)
                .ThenInclude(item => item.KichCo)
            .Where(item => item.GioHangID == cartId)
            .OrderBy(item => item.ChiTietSanPham.SanPham.Ten)
            .ThenBy(item => item.ChiTietSanPham.Mau.Ten)
            .ThenBy(item => item.ChiTietSanPham.KichCo.Ten)
            .ToListAsync(cancellationToken);

        var discounts = await GetActiveDiscountsAsync(
            items.Select(item => item.ChiTietSanPham.SanPhamID).Distinct().ToList(),
            cancellationToken);

        return new CartResponse
        {
            Items = items.Select(item => new CartItemResponse
            {
                SanPhamID = item.ChiTietSanPham.SanPhamID,
                ChiTietSanPhamID = item.ChiTietSanPhamID,
                TenSanPham = item.ChiTietSanPham.SanPham.Ten,
                PhanLoai = $"{item.ChiTietSanPham.Mau.Ten} / {item.ChiTietSanPham.KichCo.Ten.Replace("Size ", string.Empty)}",
                SoLuong = item.SoLuong,
                DonGia = ApplyDiscount(item.ChiTietSanPham.GiaNiemYet, discounts.GetValueOrDefault(item.ChiTietSanPham.SanPhamID)),
                TonKho = item.ChiTietSanPham.SoLuongTonKho
            }).ToList()
        };
    }

    private async Task<decimal> GetCurrentUnitPriceAsync(ChiTietSanPham variant, CancellationToken cancellationToken)
    {
        var discounts = await GetActiveDiscountsAsync([variant.SanPhamID], cancellationToken);
        return ApplyDiscount(variant.GiaNiemYet, discounts.GetValueOrDefault(variant.SanPhamID));
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
}