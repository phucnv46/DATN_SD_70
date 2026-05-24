using DATN_70.Data;
using DATN_70.Models.Entities;
using DATN_70.Models.Enums;
using DATN_70.Models.ViewModels;
using DATN_70.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly IStoreRepository _storeRepo;

    public HomeController(AppDbContext dbContext, IStoreRepository storeRepo)
    {
        _dbContext = dbContext;
        _storeRepo = storeRepo;
    }

    public async Task<IActionResult> Index()
    {
        // Banners
        var banners = await _dbContext.Banners
            .AsNoTracking()
            .Where(item => item.KichHoat)
            .OrderBy(item => item.ThuTu)
            .ThenBy(item => item.TieuDe)
            .Select(item => new HomeBannerViewModel
            {
                Id = item.BannerID,
                Title = item.TieuDe,
                Description = item.MoTa ?? string.Empty,
                ImageUrl = item.HinhAnhUrl,
                LinkUrl = string.IsNullOrWhiteSpace(item.LienKetUrl) ? "/Home/Products" : item.LienKetUrl
            })
            .ToListAsync();

        // Sản phẩm từ repository (đã tính khuyến mãi toàn sàn)
        var allProducts = await _storeRepo.GetProductsAsync(HttpContext.RequestAborted);

        var featuredProducts = allProducts
            .Take(8)
            .Select(sp => new HomeProductViewModel
            {
                SanPhamID = sp.SanPhamID,
                TenSanPham = sp.Ten,
                MoTaNgan = sp.MoTa,
                DanhMuc = sp.DanhMuc ?? "Lookbook",
                GiaThapNhat = sp.GiaThapNhat,
                GiaGoc = sp.GiaGoc,
                PhanTramGiam = (double)sp.PhanTramGiam,
                HinhAnhUrl = sp.HinhAnhDaiDien ?? "/images/default-product.png"
            })
            .ToList();

        var saleProducts = allProducts
            .OrderByDescending(sp => sp.PhanTramGiam)
            .Take(4)
            .Select(sp => new HomeProductViewModel
            {
                SanPhamID = sp.SanPhamID,
                TenSanPham = sp.Ten,
                MoTaNgan = sp.MoTa,
                DanhMuc = sp.DanhMuc ?? "Khuyến mãi",
                GiaThapNhat = sp.GiaThapNhat,
                GiaGoc = sp.GiaGoc,
                PhanTramGiam = (double)sp.PhanTramGiam,
                HinhAnhUrl = sp.HinhAnhDaiDien ?? "/images/default-product.png"
            })
            .ToList();

        var model = new HomeIndexViewModel
        {
            Banners = banners,
            FeaturedProducts = featuredProducts,
            SaleProducts = saleProducts
        };

        return View(model);
    }

    public async Task<IActionResult> Products(string? category, string? size, decimal? minPrice, decimal? maxPrice, string? search, string? sort)
    {
        var allProducts = await _storeRepo.GetProductsAsync(HttpContext.RequestAborted);

        if (!string.IsNullOrWhiteSpace(category) && category != "all")
            allProducts = allProducts.Where(p => p.DanhMuc == category).ToList();

        if (!string.IsNullOrWhiteSpace(search))
            allProducts = allProducts.Where(p => p.Ten.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        if (minPrice.HasValue && minPrice.Value > 0)
            allProducts = allProducts.Where(p => p.GiaThapNhat >= minPrice.Value).ToList();

        if (maxPrice.HasValue && maxPrice.Value > 0)
            allProducts = allProducts.Where(p => p.GiaThapNhat <= maxPrice.Value).ToList();

        allProducts = sort switch
        {
            "price-asc" => allProducts.OrderBy(p => p.GiaThapNhat).ToList(),
            "price-desc" => allProducts.OrderByDescending(p => p.GiaThapNhat).ToList(),
            "name-asc" => allProducts.OrderBy(p => p.Ten).ToList(),
            "name-desc" => allProducts.OrderByDescending(p => p.Ten).ToList(),
            _ => allProducts
        };

        var productViewModels = allProducts.Select(sp => new HomeProductViewModel
        {
            SanPhamID = sp.SanPhamID,
            TenSanPham = sp.Ten,
            MoTaNgan = sp.MoTa,
            DanhMuc = sp.DanhMuc ?? "Lookbook",
            GiaThapNhat = sp.GiaThapNhat,
            GiaGoc = sp.GiaGoc,
            PhanTramGiam = (double)sp.PhanTramGiam,
            HinhAnhUrl = sp.HinhAnhDaiDien ?? "/images/default-product.png"
        }).ToList();

        ViewBag.Categories = await _dbContext.DanhMucs.AsNoTracking().Select(d => new { d.DanhMucID, d.Ten }).ToListAsync();
        ViewBag.Sizes = await _dbContext.KichCos.AsNoTracking().Select(k => new { k.KichCoID, k.Ten }).ToListAsync();
        ViewBag.CurrentCategory = category ?? "all";
        ViewBag.CurrentSize = size ?? "all";
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentMinPrice = minPrice;
        ViewBag.CurrentMaxPrice = maxPrice;
        ViewBag.CurrentSort = sort;

        return View(productViewModels);
    }


    public IActionResult Details(string? id)
    {
        ViewData["ProductId"] = id ?? string.Empty;
        return View("~/Views/ChiTietSanPham/Index.cshtml");
    }

    public IActionResult Cart()
    {
        if (!IsAuthenticated())
        {
            return RedirectToAction("Login", "Account");
        }

        return View("~/Views/GioHang/Index.cshtml");
    }

    public async Task<IActionResult> Checkout()
    {
        if (!IsAuthenticated())
        {
            return RedirectToAction("Login", "Account");
        }
        var userId = HttpContext.Session.GetString("UserId");
        var account = await _dbContext.TaiKhoans
            .AsNoTracking()
            .Include(t => t.KhachHang)
            .FirstOrDefaultAsync(t => t.TaiKhoanID == userId);

        ViewBag.RealKhachHangId = account?.KhachHang?.KhachHangID ?? "";

        // 🔥 ĐÃ FIX LỖI TẠI ĐÂY: Truy vấn đúng chuẩn các cột của thực thể KhuyenMai
        ViewBag.DbCoupons = await _dbContext.KhuyenMais
            .AsNoTracking()
            .Where(v => v.MaCode != null && v.MaCode != ""
                     && v.NgayApDung <= DateTime.Now
                     && v.NgayKetThuc >= DateTime.Now
                     && v.SoLuongDaDung < v.SoLuongToiDa) // Chỉ lấy mã còn lượt dùng và còn hạn
            .Select(v => new
            {
                MaCode = v.MaCode,
                Ten = v.Ten,
                MoTa = v.MoTa ?? string.Empty,
                GiaTriGiam = v.GiaTriGiam,
                LoaiGiamGia = (int)v.LoaiGiamGia // Ép kiểu Enum về Int (0: Tiền mặt, 1: Phần trăm)
            })
            .ToListAsync();

        var model = new CheckoutPageViewModel
        {
            Customer = await BuildCheckoutCustomerAsync()
        };

        return View("~/Views/PhuongThucThanhToan/Index.cshtml", model);
    }

    private async Task<CheckoutCustomerBootstrapViewModel> BuildCheckoutCustomerAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new CheckoutCustomerBootstrapViewModel();
        }

        var account = await _dbContext.TaiKhoans
            .AsNoTracking()
            .Include(item => item.KhachHang)
            .FirstOrDefaultAsync(item => item.TaiKhoanID == userId);

        if (account is null)
        {
            return new CheckoutCustomerBootstrapViewModel();
        }

        var customer = account.KhachHang;
        if (customer is null)
        {
            return new CheckoutCustomerBootstrapViewModel
            {
                IsAuthenticated = true,
                Email = account.Email
            };
        }

        var addressEntities = await _dbContext.DiaChis
            .AsNoTracking()
            .Where(item => item.KhachHangID == customer.KhachHangID)
            .OrderByDescending(item => item.LaMacDinh)
            .ThenBy(item => item.TenNguoiNhan)
            .ToListAsync();

        var addresses = addressEntities.Select(item => new SavedAddressViewModel
        {
            Id = item.DiaChiID,
            RecipientName = item.TenNguoiNhan,
            Phone = item.SoDienThoaiNhan,
            Street = AddressSerializer.ExtractStreet(item.PhuongXa),
            Ward = AddressSerializer.ExtractWard(item.PhuongXa),
            District = item.QuanHuyen,
            Province = item.TinhThanh,
            IsDefault = item.LaMacDinh
        }).ToList();

        var defaultAddress = addresses.FirstOrDefault(item => item.IsDefault) ?? addresses.FirstOrDefault();

        return new CheckoutCustomerBootstrapViewModel
        {
            IsAuthenticated = true,
            FullName = customer.Ten ?? string.Empty,
            Email = account.Email ?? customer.Email ?? string.Empty,
            Phone = customer.SoDienThoai ?? string.Empty,
            Street = defaultAddress?.Street ?? ExtractStreetFromCustomer(customer.DiaChi),
            Province = defaultAddress?.Province ?? string.Empty,
            District = defaultAddress?.District ?? string.Empty,
            Ward = defaultAddress?.Ward ?? string.Empty,
            SelectedAddressId = defaultAddress?.Id ?? string.Empty,
            SavedAddresses = addresses
        };
    }

    private static string ExtractStreetFromCustomer(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return string.Empty;
        }

        var parts = address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.FirstOrDefault() ?? address;
    }

    private bool IsAuthenticated()
    {
        return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId"));
    }
    private async Task<decimal> GetDiscountedPriceAsync(ChiTietSanPham variant, CancellationToken ct)
    {
        var basePrice = variant.GiaNiemYet;
        decimal bestPrice = basePrice;
        var now = DateTime.Now;

        // 1. Khuyến mãi riêng
        var productPromo = await _dbContext.KhuyenMaiSanPhams
            .Include(ksp => ksp.KhuyenMai)
            .Where(ksp => ksp.SanPhamID == variant.SanPhamID
                && ksp.KhuyenMai.TrangThai == Enums.TrangThaiHoatDong.HoatDong
                && ksp.KhuyenMai.NgayApDung <= now && ksp.KhuyenMai.NgayKetThuc >= now
                && (ksp.KhuyenMai.SoLuongToiDa == 0 || ksp.KhuyenMai.SoLuongDaDung < ksp.KhuyenMai.SoLuongToiDa))
            .Select(ksp => ksp.KhuyenMai)
            .FirstOrDefaultAsync(ct);

        if (productPromo != null)
        {
            var price = ApplyDiscount(basePrice, productPromo);
            if (price < bestPrice) bestPrice = price;
        }

        // 2. Khuyến mãi toàn sàn
        var globalPromo = await _dbContext.KhuyenMais
            .FirstOrDefaultAsync(k =>
                k.MaCode == null &&
                k.TrangThai == Enums.TrangThaiHoatDong.HoatDong &&
                k.NgayApDung <= now && k.NgayKetThuc >= now &&
                (k.SoLuongToiDa == 0 || k.SoLuongDaDung < k.SoLuongToiDa) &&
                !k.KhuyenMaiSanPhams.Any(),
            ct);

        if (globalPromo != null)
        {
            var price = ApplyDiscount(basePrice, globalPromo);
            if (price < bestPrice) bestPrice = price;
        }

        return bestPrice;
    }

    // Hàm tính giá sau khuyến mãi (giống CartController)
    private static decimal ApplyDiscount(decimal basePrice, KhuyenMai? promo)
    {
        if (promo == null || promo.GiaTriGiam <= 0)
            return basePrice;

        decimal tienGiam = 0;
        if (promo.LoaiGiamGia == Enums.LoaiGiamGia.PhanTram) // giảm %
        {
            tienGiam = basePrice * (promo.GiaTriGiam / 100m);
            if (promo.GiamToiDa > 0 && tienGiam > promo.GiamToiDa)
                tienGiam = promo.GiamToiDa;
        }
        else // giảm tiền thẳng
        {
            tienGiam = promo.GiaTriGiam;
        }

        decimal final = basePrice - tienGiam;
        return final < 0 ? 0 : Math.Round(final, 0, MidpointRounding.AwayFromZero);
    }
    // ========================================================

    // API TẠO ĐƠN HÀNG CÔNG KHAI DÀNH CHO KHÁCH HÀNG TRÊN WEB (BẢN CHUẨN SẠCH LỖI BIÊN DỊCH)
    // ========================================================
    [HttpPost("/api/web-orders")]
    public async Task<IActionResult> CreateWebOrder([FromBody] WebOrderRequestModel request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest(new { message = "Giỏ hàng của bạn đang trống." });

        var payOSClient = HttpContext.RequestServices.GetService(typeof(PayOS.PayOSClient)) as PayOS.PayOSClient;

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            string finalKhachHangId = request.KhachHangID;
            var khachHangExists = await _dbContext.KhachHangs.AnyAsync(k => k.KhachHangID == finalKhachHangId, cancellationToken);
            if (!khachHangExists)
            {
                var khByTaiKhoan = await _dbContext.KhachHangs.FirstOrDefaultAsync(k => k.TaiKhoanID == finalKhachHangId, cancellationToken);
                if (khByTaiKhoan != null)
                {
                    finalKhachHangId = khByTaiKhoan.KhachHangID;
                }
                else
                {
                    var walkIn = await _dbContext.KhachHangs.FindAsync(new object[] { "KH000000" }, cancellationToken);
                    if (walkIn == null)
                    {
                        _dbContext.KhachHangs.Add(new KhachHang
                        {
                            KhachHangID = "KH000000",
                            Ten = "Khách vãng lai Web",
                            SoDienThoai = "0000000000",
                            Email = "khachweb@winterfashion.local",
                            GioiTinh = Enums.GioiTinh.Khac,
                            DiaChi = "Đặt hàng trực tuyến",
                            DiemTichLuy = 0,
                            TaiKhoanID = "TK_POS_SYSTEM"
                        });
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }
                    finalKhachHangId = "KH000000";
                }
            }

            string finalDiaChiId = request.DiaChiID;
            if (string.IsNullOrWhiteSpace(finalDiaChiId))
            {
                var defaultAddress = await _dbContext.DiaChis.FirstOrDefaultAsync(d => d.KhachHangID == finalKhachHangId, cancellationToken);
                finalDiaChiId = defaultAddress?.DiaChiID ?? "DC_POS_SYSTEM";
            }

            if (finalDiaChiId == "DC_POS_SYSTEM")
            {
                var posAddress = await _dbContext.DiaChis.FindAsync(new object[] { "DC_POS_SYSTEM" }, cancellationToken);
                if (posAddress == null)
                {
                    _dbContext.DiaChis.Add(new DiaChi
                    {
                        DiaChiID = "DC_POS_SYSTEM",
                        KhachHangID = "KH000000",
                        TenNguoiNhan = "Khách mua hệ thống",
                        SoDienThoaiNhan = "0000000000",
                        TinhThanh = "Hà Nội",
                        QuanHuyen = "Sơn Tây",
                        PhuongXa = "Hệ thống||Hệ thống",
                        LaMacDinh = true
                    });
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            var hoaDonId = "WEB" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            decimal tongTienHang = 0;
            decimal tongTienVAT = 0;
            var chiTietList = new List<HoaDonChiTiet>();

            foreach (var item in request.Items)
            {
                var ctsp = await _dbContext.ChiTietSanPhams
                    .Include(c => c.SanPham)
                    .FirstOrDefaultAsync(c => c.ChiTietSanPhamID == item.ChiTietSanPhamID, cancellationToken);

                if (ctsp == null) return BadRequest(new { message = $"Không tìm thấy sản phẩm." });
                if (ctsp.SoLuongTonKho < item.SoLuong) return BadRequest(new { message = $"Sản phẩm {ctsp.SanPham.Ten} không đủ số lượng tồn kho." });

                ctsp.SoLuongTonKho -= item.SoLuong;

                decimal mucVAT = ctsp.SanPham.MucVAT;
                decimal donGia = await GetDiscountedPriceAsync(ctsp, cancellationToken);
                decimal tienVatItem = (donGia * mucVAT / 100) * item.SoLuong;

                tongTienHang += donGia * item.SoLuong;
                tongTienVAT += tienVatItem;

                chiTietList.Add(new HoaDonChiTiet
                {
                    // 🌟 ĐÃ XÓA DÒNG BaseHoaDonChiTietID LỖI TẠI ĐÂY
                    HoaDonChiTietID = Guid.NewGuid().ToString(),
                    HoaDonID = hoaDonId,
                    ChiTietSanPhamID = ctsp.ChiTietSanPhamID,
                    SoLuong = item.SoLuong,
                    DonGia = donGia,
                    MucVAT = mucVAT,
                    TienVAT = tienVatItem
                });
            }

            decimal tongTienGiamGia = 0;
            if (!string.IsNullOrWhiteSpace(request.MaGiamGia))
            {
                var km = await _dbContext.KhuyenMais.FirstOrDefaultAsync(k => k.MaCode == request.MaGiamGia, cancellationToken);
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
                LoaiGiaoDich = Enums.LoaiGiaoDich.Online,
                TrangThai = Enums.TrangThaiHoaDon.ChoDuyet,
                GhiChu = string.IsNullOrWhiteSpace(request.DiaChiGiaoHang) ? "Đặt hàng qua website" : $"Giao tới: {request.DiaChiGiaoHang}",
                KhachHangID = finalKhachHangId,
                DiaChiID = finalDiaChiId,
                NhanVienID = finalNhanVienId,
                KhuyenMaiID = await _dbContext.KhuyenMais.Where(k => k.MaCode == request.MaGiamGia).Select(k => k.KhuyenMaiID).FirstOrDefaultAsync(cancellationToken),
                NgayTao = DateTime.Now
            };

            var activePhuongThucId = await _dbContext.Set<PhuongThucThanhToan>().Select(p => p.PhuongThucThanhToanID).FirstOrDefaultAsync(cancellationToken) ?? "PT_DEFAULT";
            long payOsOrderCode = long.Parse(DateTime.Now.ToString("ddHHmmss"));

            var cttt = new ChiTietThanhToan
            {
                ChiTietThanhToanID = Guid.NewGuid().ToString(),
                HoaDonID = hoaDonId,
                Ten = request.PhuongThucThanhToan == "QR" ? "Chuyển khoản QR" : "Tiền mặt (COD)",
                SoTien = thanhTienCuoiCung,
                MaThamChieu = request.PhuongThucThanhToan == "QR" ? payOsOrderCode.ToString() : string.Empty,
                TrangThai = Enums.TrangThaiThanhToan.ThatBai,
                PhuongThucThanhToanID = activePhuongThucId
            };

            _dbContext.HoaDons.Add(hoaDon);
            _dbContext.HoaDonChiTiets.AddRange(chiTietList);
            _dbContext.Set<ChiTietThanhToan>().Add(cttt);
            await _dbContext.SaveChangesAsync(cancellationToken);

            string payOsCheckoutUrl = "";
            if (request.PhuongThucThanhToan == "QR" && payOSClient != null)
            {
                // 🌟 FIX ĐƯỜNG DẪN LỚP: Gọi đúng chuẩn không gian tên phân cấp V2 của PayOS
                var paymentRequest = new PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest
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

            return Ok(new
            {
                message = "Đặt hàng thành công!",
                hoaDonID = hoaDon.HoaDonID,
                kieuThanhToan = request.PhuongThucThanhToan == "QR" ? 1 : 0,
                checkoutUrl = payOsCheckoutUrl
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return StatusCode(500, new { message = "Lỗi hệ thống lưu đơn hàng.", error = ex.Message });
        }
    }
    [HttpGet]
    public IActionResult Success(string orderId)
    {
        ViewBag.OrderId = orderId;
        return View();
    }

    // ========================================================
    // CÁC LỚP PHỤ TRỢ (NẰM ĐỘC LẬP BÊN NGOÀI CLASS HOMECONTROLLER)
    // ========================================================
    internal static class AddressSerializer
    {
        private const string Separator = "||";

        public static string Pack(string ward, string street)
        {
            return $"{ward}{Separator}{street}";
        }

        public static string ExtractWard(string? packedWard)
        {
            if (string.IsNullOrWhiteSpace(packedWard)) return string.Empty;
            var parts = packedWard.Split(Separator, StringSplitOptions.None);
            return parts[0];
        }

        public static string ExtractStreet(string? packedWard)
        {
            if (string.IsNullOrWhiteSpace(packedWard)) return string.Empty;
            var parts = packedWard.Split(Separator, StringSplitOptions.None);
            return parts.Length > 1 ? parts[1] : string.Empty;
        }
    }

    public class WebOrderRequestModel
    {
        public string KhachHangID { get; set; } = string.Empty;
        public string? DiaChiID { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public string PhuongThucThanhToan { get; set; } = "COD";
        public string? MaGiamGia { get; set; }
        public List<WebOrderItemDto> Items { get; set; } = new();
    }

    public class WebOrderItemDto
    {
        public string ChiTietSanPhamID { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }
}