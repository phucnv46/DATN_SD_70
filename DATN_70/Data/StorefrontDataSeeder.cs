using DATN_70.Models.Entities;
using DATN_70.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Data;

public sealed class StorefrontDataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StorefrontDataSeeder> _logger;

    public StorefrontDataSeeder(
        AppDbContext dbContext,
        ILogger<StorefrontDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureBannerTableAsync(cancellationToken);
        await SeedBannerAsync(cancellationToken);
        await SeedVaiTroAsync(cancellationToken);
        await SeedDanhMucAsync(cancellationToken);
        await SeedThuongHieuAsync(cancellationToken);
        await SeedKichCoAsync(cancellationToken);
        await SeedMauAsync(cancellationToken);
        await SeedSanPhamAsync(cancellationToken);
        await SeedChiTietSanPhamAsync(cancellationToken);
        await SeedKhuyenMaiAsync(cancellationToken);
        await SeedTaiKhoanMauAsync(cancellationToken);
        await SeedKhachHangMauAsync(cancellationToken);
        await SeedDiaChiMauAsync(cancellationToken);

        _logger.LogInformation("Storefront seed data is ready with extended demo dataset.");
    }

    private async Task EnsureBannerTableAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF OBJECT_ID(N'dbo.Banners', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Banners (
                    BannerID nvarchar(36) NOT NULL PRIMARY KEY,
                    TieuDe nvarchar(160) NOT NULL,
                    MoTa nvarchar(300) NULL,
                    HinhAnhUrl nvarchar(500) NOT NULL,
                    LienKetUrl nvarchar(300) NULL,
                    ThuTu int NOT NULL,
                    KichHoat bit NOT NULL
                );
            END
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task SeedBannerAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new Banner
            {
                BannerID = "BNR-001",
                TieuDe = "Áo khoác mùa lạnh 2026",
                MoTa = "Bộ sưu tập áo khoác mới cho mùa lạnh, tập trung vào chất liệu đẹp và tính thực dụng.",
                HinhAnhUrl = "https://images.pexels.com/photos/6311392/pexels-photo-6311392.jpeg?auto=compress&cs=tinysrgb&w=1600",
                LienKetUrl = "/Home/Products?category=outerwear",
                ThuTu = 1,
                KichHoat = true
            },
            new Banner
            {
                BannerID = "BNR-002",
                TieuDe = "Phối lớp mùa lạnh",
                MoTa = "Len, hoodie và các lớp mặc trong được cập nhật theo tiêu chí phối đồ nhanh và gọn.",
                HinhAnhUrl = "https://images.pexels.com/photos/7691128/pexels-photo-7691128.jpeg?auto=compress&cs=tinysrgb&w=1600",
                LienKetUrl = "/Home/Products?category=layering",
                ThuTu = 2,
                KichHoat = true
            }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.Banners.FirstOrDefaultAsync(x => x.BannerID == item.BannerID, cancellationToken);
            if (existing is null)
            {
                _dbContext.Banners.Add(item);
            }
            else
            {
                existing.TieuDe = item.TieuDe;
                existing.MoTa = item.MoTa;
                existing.HinhAnhUrl = item.HinhAnhUrl;
                existing.LienKetUrl = item.LienKetUrl;
                existing.ThuTu = item.ThuTu;
                existing.KichHoat = item.KichHoat;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedVaiTroAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new VaiTro { VaiTroID = "R01", Ten = "Quản trị viên" },
            new VaiTro { VaiTroID = "R02", Ten = "Nhân viên" },
            new VaiTro { VaiTroID = "R03", Ten = "Khách hàng" }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.VaiTros.FirstOrDefaultAsync(x => x.VaiTroID == item.VaiTroID, cancellationToken);
            if (existing is null)
            {
                _dbContext.VaiTros.Add(item);
            }
            else
            {
                existing.Ten = item.Ten;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDanhMucAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new DanhMuc { DanhMucID = "DM001", Ten = "Áo khoác phao" },
            new DanhMuc { DanhMucID = "DM002", Ten = "Áo khoác da" },
            new DanhMuc { DanhMucID = "DM003", Ten = "Áo len và hoodie" },
            new DanhMuc { DanhMucID = "DM004", Ten = "Măng tô và parka" },
            new DanhMuc { DanhMucID = "DM005", Ten = "Áo giữ nhiệt" },
            new DanhMuc { DanhMucID = "DM006", Ten = "Gile và bomber" }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.DanhMucs.FirstOrDefaultAsync(x => x.DanhMucID == item.DanhMucID, cancellationToken);
            if (existing is null)
            {
                _dbContext.DanhMucs.Add(item);
            }
            else
            {
                existing.Ten = item.Ten;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedThuongHieuAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new ThuongHieu { ThuongHieuID = "TH001", Ten = "Arctic Wear", LogoURL = "", MoTa = "Thương hiệu tập trung vào áo khoác giữ nhiệt và đồ ngoài trời." },
            new ThuongHieu { ThuongHieuID = "TH002", Ten = "Urban Style", LogoURL = "", MoTa = "Phong cách đường phố gọn gàng, dễ phối hằng ngày." },
            new ThuongHieu { ThuongHieuID = "TH003", Ten = "North Cabin", LogoURL = "", MoTa = "Các thiết kế len, nỉ và layering cho thời tiết lạnh." },
            new ThuongHieu { ThuongHieuID = "TH004", Ten = "Mono Heat", LogoURL = "", MoTa = "Dòng sản phẩm heattech cơ bản, nhẹ và ôm gọn." },
            new ThuongHieu { ThuongHieuID = "TH005", Ten = "Field Motion", LogoURL = "", MoTa = "Trang phục di chuyển ngoài trời, chống gió và chống nước nhẹ." }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.ThuongHieus.FirstOrDefaultAsync(x => x.ThuongHieuID == item.ThuongHieuID, cancellationToken);
            if (existing is null)
            {
                _dbContext.ThuongHieus.Add(item);
            }
            else
            {
                existing.Ten = item.Ten;
                existing.LogoURL = item.LogoURL;
                existing.MoTa = item.MoTa;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedKichCoAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new KichCo { KichCoID = "SZS", Ten = "Size S", MoTa = "Phù hợp vóc dáng nhỏ gọn." },
            new KichCo { KichCoID = "SZM", Ten = "Size M", MoTa = "Dáng tiêu chuẩn dễ mặc." },
            new KichCo { KichCoID = "SZL", Ten = "Size L", MoTa = "Thoải mái cho layering." },
            new KichCo { KichCoID = "SZX", Ten = "Size XL", MoTa = "Form rộng cho ngày lạnh sâu." },
            new KichCo { KichCoID = "SZ2", Ten = "Size XXL", MoTa = "Dành cho phom rộng hoặc mặc nhiều lớp." }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.KichCos.FirstOrDefaultAsync(x => x.KichCoID == item.KichCoID, cancellationToken);
            if (existing is null)
            {
                _dbContext.KichCos.Add(item);
            }
            else
            {
                existing.Ten = item.Ten;
                existing.MoTa = item.MoTa;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedMauAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new Mau { MauID = "BLK", Ten = "Đen Onyx" },
            new Mau { MauID = "CRM", Ten = "Kem Cashmere" },
            new Mau { MauID = "GRN", Ten = "Xanh rêu" },
            new Mau { MauID = "BRN", Ten = "Nâu Cocoa" },
            new Mau { MauID = "GRY", Ten = "Ghi khói" },
            new Mau { MauID = "NVY", Ten = "Xanh navy" },
            new Mau { MauID = "WHT", Ten = "Trắng tuyết" },
            new Mau { MauID = "RUS", Ten = "Đỏ gạch" }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.Maus.FirstOrDefaultAsync(x => x.MauID == item.MauID, cancellationToken);
            if (existing is null)
            {
                _dbContext.Maus.Add(item);
            }
            else
            {
                existing.Ten = item.Ten;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSanPhamAsync(CancellationToken cancellationToken)
    {
        var items = GetProductSeeds();

        foreach (var item in items)
        {
            var existing = await _dbContext.SanPhams.FirstOrDefaultAsync(x => x.SanPhamID == item.SanPhamID, cancellationToken);
            if (existing is null)
            {
                _dbContext.SanPhams.Add(new SanPham
                {
                    SanPhamID = item.SanPhamID,
                    Ten = item.Ten,
                    ChatLieu = item.ChatLieu,
                    DanhMucID = item.DanhMucID,
                    ThuongHieuID = item.ThuongHieuID,
                    MoTa = item.MoTa,
                    MucVAT = 10
                });
            }
            else
            {
                existing.Ten = item.Ten;
                existing.ChatLieu = item.ChatLieu;
                existing.DanhMucID = item.DanhMucID;
                existing.ThuongHieuID = item.ThuongHieuID;
                existing.MoTa = item.MoTa;
                existing.MucVAT = 10;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedChiTietSanPhamAsync(CancellationToken cancellationToken)
    {
        var items = GetVariantSeeds();

        foreach (var item in items)
        {
            var existing = await _dbContext.ChiTietSanPhams.FirstOrDefaultAsync(x => x.ChiTietSanPhamID == item.Id, cancellationToken);
            if (existing is null)
            {
                _dbContext.ChiTietSanPhams.Add(new ChiTietSanPham
                {
                    ChiTietSanPhamID = item.Id,
                    SanPhamID = item.SanPhamID,
                    KichCoID = item.KichCoID,
                    MauID = item.MauID,
                    SKU = item.Sku,
                    GiaNiemYet = item.GiaNiemYet,
                    SoLuongTonKho = item.SoLuongTon
                });
            }
            else
            {
                existing.SanPhamID = item.SanPhamID;
                existing.KichCoID = item.KichCoID;
                existing.MauID = item.MauID;
                existing.SKU = item.Sku;
                existing.GiaNiemYet = item.GiaNiemYet;
                existing.SoLuongTonKho = item.SoLuongTon;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTaiKhoanMauAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new TaiKhoan { TaiKhoanID = "TK0001", Email = "an.nguyen@wintershop.vn", MatKhau = "123456", TrangThai = "Hoạt động", VaiTroID = "R03" },
            new TaiKhoan { TaiKhoanID = "TK0002", Email = "minh.tran@wintershop.vn", MatKhau = "123456", TrangThai = "Hoạt động", VaiTroID = "R03" },
            new TaiKhoan { TaiKhoanID = "TK0003", Email = "thu.le@wintershop.vn", MatKhau = "123456", TrangThai = "Hoạt động", VaiTroID = "R03" },
            new TaiKhoan { TaiKhoanID = "TK0004", Email = "duy.pham@wintershop.vn", MatKhau = "123456", TrangThai = "Hoạt động", VaiTroID = "R03" },
            new TaiKhoan { TaiKhoanID = "TK0005", Email = "linh.do@wintershop.vn", MatKhau = "123456", TrangThai = "Hoạt động", VaiTroID = "R03" }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.TaiKhoans.FirstOrDefaultAsync(x => x.TaiKhoanID == item.TaiKhoanID, cancellationToken);
            if (existing is null)
            {
                _dbContext.TaiKhoans.Add(item);
            }
            else
            {
                existing.Email = item.Email;
                existing.MatKhau = item.MatKhau;
                existing.TrangThai = item.TrangThai;
                existing.VaiTroID = item.VaiTroID;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedKhachHangMauAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new KhachHang { KhachHangID = "KH0001", Ten = "Nguyễn Hoài An", Email = "an.nguyen@wintershop.vn", SoDienThoai = "0912345678", GioiTinh = Enums.GioiTinh.Nu, DiaChi = "22 Nguyễn Trãi, Bến Thành, Quận 1, TP.HCM", TaiKhoanID = "TK0001" },
            new KhachHang { KhachHangID = "KH0002", Ten = "Trần Quốc Minh", Email = "minh.tran@wintershop.vn", SoDienThoai = "0987654321", GioiTinh = Enums.GioiTinh.Nam, DiaChi = "89 Cầu Giấy, Dịch Vọng, Cầu Giấy, Hà Nội", TaiKhoanID = "TK0002" },
            new KhachHang { KhachHangID = "KH0003", Ten = "Lê Bảo Thư", Email = "thu.le@wintershop.vn", SoDienThoai = "0976123456", GioiTinh = Enums.GioiTinh.Nu, DiaChi = "14 Lê Duẩn, Hải Châu 1, Hải Châu, Đà Nẵng", TaiKhoanID = "TK0003" },
            new KhachHang { KhachHangID = "KH0004", Ten = "Phạm Gia Duy", Email = "duy.pham@wintershop.vn", SoDienThoai = "0934567890", GioiTinh = Enums.GioiTinh.Nam, DiaChi = "38 Trần Phú, Ngô Quyền, Hải Phòng", TaiKhoanID = "TK0004" },
            new KhachHang { KhachHangID = "KH0005", Ten = "Đỗ Mỹ Linh", Email = "linh.do@wintershop.vn", SoDienThoai = "0391122334", GioiTinh = Enums.GioiTinh.Nu, DiaChi = "11 Hùng Vương, Ninh Kiều, Cần Thơ", TaiKhoanID = "TK0005" }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.KhachHangs.FirstOrDefaultAsync(x => x.KhachHangID == item.KhachHangID, cancellationToken);
            if (existing is null)
            {
                _dbContext.KhachHangs.Add(item);
            }
            else
            {
                existing.Ten = item.Ten;
                existing.Email = item.Email;
                existing.SoDienThoai = item.SoDienThoai;
                existing.GioiTinh = item.GioiTinh;
                existing.DiaChi = item.DiaChi;
                existing.TaiKhoanID = item.TaiKhoanID;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDiaChiMauAsync(CancellationToken cancellationToken)
    {
        var items = new[]
        {
            new DiaChi { DiaChiID = "DC0001", KhachHangID = "KH0001", TenNguoiNhan = "Nguyễn Hoài An", SoDienThoaiNhan = "0912345678", TinhThanh = "TP.HCM", QuanHuyen = "Quận 1", PhuongXa = PackAddress("Bến Thành", "22 Nguyễn Trãi"), LaMacDinh = true },
            new DiaChi { DiaChiID = "DC0002", KhachHangID = "KH0001", TenNguoiNhan = "Nguyễn Hoài An", SoDienThoaiNhan = "0912345678", TinhThanh = "TP.HCM", QuanHuyen = "Quận 3", PhuongXa = PackAddress("Phường Võ Thị Sáu", "144 Nam Kỳ Khởi Nghĩa"), LaMacDinh = false },
            new DiaChi { DiaChiID = "DC0003", KhachHangID = "KH0002", TenNguoiNhan = "Trần Quốc Minh", SoDienThoaiNhan = "0987654321", TinhThanh = "Hà Nội", QuanHuyen = "Cầu Giấy", PhuongXa = PackAddress("Dịch Vọng", "89 Cầu Giấy"), LaMacDinh = true },
            new DiaChi { DiaChiID = "DC0004", KhachHangID = "KH0002", TenNguoiNhan = "Trần Quốc Minh", SoDienThoaiNhan = "0987654321", TinhThanh = "Hà Nội", QuanHuyen = "Ba Đình", PhuongXa = PackAddress("Điện Biên", "18 Hoàng Diệu"), LaMacDinh = false },
            new DiaChi { DiaChiID = "DC0005", KhachHangID = "KH0003", TenNguoiNhan = "Lê Bảo Thư", SoDienThoaiNhan = "0976123456", TinhThanh = "Đà Nẵng", QuanHuyen = "Hải Châu", PhuongXa = PackAddress("Hải Châu 1", "14 Lê Duẩn"), LaMacDinh = true },
            new DiaChi { DiaChiID = "DC0006", KhachHangID = "KH0004", TenNguoiNhan = "Phạm Gia Duy", SoDienThoaiNhan = "0934567890", TinhThanh = "Hải Phòng", QuanHuyen = "Ngô Quyền", PhuongXa = PackAddress("Lạch Tray", "38 Trần Phú"), LaMacDinh = true },
            new DiaChi { DiaChiID = "DC0007", KhachHangID = "KH0005", TenNguoiNhan = "Đỗ Mỹ Linh", SoDienThoaiNhan = "0391122334", TinhThanh = "Cần Thơ", QuanHuyen = "Ninh Kiều", PhuongXa = PackAddress("Tân An", "11 Hùng Vương"), LaMacDinh = true }
        };

        foreach (var item in items)
        {
            var existing = await _dbContext.DiaChis.FirstOrDefaultAsync(x => x.DiaChiID == item.DiaChiID, cancellationToken);
            if (existing is null)
            {
                _dbContext.DiaChis.Add(item);
            }
            else
            {
                existing.KhachHangID = item.KhachHangID;
                existing.TenNguoiNhan = item.TenNguoiNhan;
                existing.SoDienThoaiNhan = item.SoDienThoaiNhan;
                existing.TinhThanh = item.TinhThanh;
                existing.QuanHuyen = item.QuanHuyen;
                existing.PhuongXa = item.PhuongXa;
                existing.LaMacDinh = item.LaMacDinh;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedKhuyenMaiAsync(CancellationToken cancellationToken)
    {
        var promotions = new[]
        {
            new KhuyenMai
            {
                KhuyenMaiID = "KM001",
                Ten = "Outerwear Deep Sale",
                PhanTramChietKhau = 30,
                GiaTriToiThieuApDung = 0,
                NgayApDung = DateTime.Today.AddDays(-30),
                NgayKetThuc = DateTime.Today.AddDays(60),
                MoTa = "Khuyen mai demo cho nhom ao khoac duoc cho phep giam gia.",
                TrangThai = Models.Enums.Enums.TrangThaiHoatDong.HoatDong
            },
            new KhuyenMai
            {
                KhuyenMaiID = "KM002",
                Ten = "Heattech Promo",
                PhanTramChietKhau = 15,
                GiaTriToiThieuApDung = 0,
                NgayApDung = DateTime.Today.AddDays(-15),
                NgayKetThuc = DateTime.Today.AddDays(45),
                MoTa = "Khuyen mai demo cho nhom giu nhiet.",
                TrangThai = Models.Enums.Enums.TrangThaiHoatDong.HoatDong
            }
        };

        foreach (var item in promotions)
        {
            var existing = await _dbContext.KhuyenMais.FirstOrDefaultAsync(x => x.KhuyenMaiID == item.KhuyenMaiID, cancellationToken);
            if (existing is null)
            {
                _dbContext.KhuyenMais.Add(item);
            }
            else
            {
                existing.Ten = item.Ten;
                existing.PhanTramChietKhau = item.PhanTramChietKhau;
                existing.GiaTriToiThieuApDung = item.GiaTriToiThieuApDung;
                existing.NgayApDung = item.NgayApDung;
                existing.NgayKetThuc = item.NgayKetThuc;
                existing.MoTa = item.MoTa;
                existing.TrangThai = item.TrangThai;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var links = new[]
        {
            new KhuyenMaiSanPham { KhuyenMaiID = "KM001", SanPhamID = "SP0001" },
            new KhuyenMaiSanPham { KhuyenMaiID = "KM001", SanPhamID = "SP0010" },
            new KhuyenMaiSanPham { KhuyenMaiID = "KM001", SanPhamID = "SP0019" },
            new KhuyenMaiSanPham { KhuyenMaiID = "KM002", SanPhamID = "SP0009" },
            new KhuyenMaiSanPham { KhuyenMaiID = "KM002", SanPhamID = "SP0023" }
        };

        foreach (var item in links)
        {
            var exists = await _dbContext.KhuyenMaiSanPhams.AnyAsync(
                x => x.KhuyenMaiID == item.KhuyenMaiID && x.SanPhamID == item.SanPhamID,
                cancellationToken);

            if (!exists)
            {
                _dbContext.KhuyenMaiSanPhams.Add(item);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<ProductSeed> GetProductSeeds()
    {
        return new[]
        {
            new ProductSeed("SP0001", "Áo Phao Arctic Shield", "Vải phao chống nước", "DM001", "TH001", "Form dài, giữ nhiệt tốt và cản gió nhẹ cho ngày lạnh sâu."),
            new ProductSeed("SP0002", "Áo Da Sherpa Espresso", "Da tổng hợp lót lông", "DM002", "TH002", "Dáng gọn, hợp đi làm và phối cùng boots mùa đông."),
            new ProductSeed("SP0003", "Sweater Alpine Soft Knit", "Len dệt mềm", "DM003", "TH003", "Mẫu sweater mặc một lớp hoặc layering đều gọn."),
            new ProductSeed("SP0004", "Áo Măng Tô Wool Blend", "Len pha dạ", "DM004", "TH001", "Măng tô dáng suông, tông màu trầm dễ lên outfit công sở."),
            new ProductSeed("SP0005", "Hoodie Fleece Cloudline", "Nỉ fleece", "DM003", "TH003", "Giữ ấm nhanh, hợp mặc hằng ngày và phối áo khoác ngoài."),
            new ProductSeed("SP0006", "Gile Phao Urban Heat", "Phao gòn nhẹ", "DM006", "TH002", "Layering nhanh với sơ mi, len hoặc hoodie."),
            new ProductSeed("SP0007", "Áo Khoác Metro Zip", "Poly chống gió", "DM006", "TH002", "Bom dáng ngắn, linh hoạt khi di chuyển trong thành phố."),
            new ProductSeed("SP0008", "Áo Len Merino Ease", "Merino blend", "DM003", "TH003", "Áo len mỏng nhẹ, giữ nhiệt vừa phải và không bí."),
            new ProductSeed("SP0009", "Áo Giữ Nhiệt Core Warm", "Heattech co giãn", "DM005", "TH004", "Mặc lót bên trong áo sơ mi hoặc hoodie đều ổn."),
            new ProductSeed("SP0010", "Parka Snow Ranger", "Canvas phủ chống nước", "DM004", "TH005", "Parka mũ lông nhân tạo, phù hợp chuyến đi xa ngày lạnh."),
            new ProductSeed("SP0011", "Cardigan Layer Softline", "Len pha cotton", "DM003", "TH003", "Cardigan dáng mở, lên form thanh lịch và nhẹ nhàng."),
            new ProductSeed("SP0012", "Bomber Frost Street", "Poly dày", "DM006", "TH002", "Bomber trẻ trung, dễ đi cùng quần denim và boots."),
            new ProductSeed("SP0013", "Áo Phao Glacier Run", "Phao lì chống thấm", "DM001", "TH001", "Thiết kế phồng vừa phải, gọn vai và giữ ấm tốt."),
            new ProductSeed("SP0014", "Áo Da Midnight Rider", "Da tổng hợp mờ", "DM002", "TH002", "Phong cách đường phố, cổ dựng và bo gấu chắc chắn."),
            new ProductSeed("SP0015", "Hoodie Cabin Relax", "Nỉ chải bông", "DM003", "TH003", "Form rộng vừa, hợp phối cùng gile hoặc bomber."),
            new ProductSeed("SP0016", "Măng Tô Smoke Tailor", "Dạ pha wool", "DM004", "TH001", "Bề mặt mịn, tạo cảm giác đứng dáng khi mặc đi làm."),
            new ProductSeed("SP0017", "Heattech Slim Base", "Sợi giữ nhiệt nhẹ", "DM005", "TH004", "Co giãn tốt, ôm thân mà vẫn dễ chịu khi mặc lâu."),
            new ProductSeed("SP0018", "Gile Wind Trail", "Vải gió chần bông", "DM006", "TH005", "Gile thể thao, chống gió nhẹ và phối đồ nhanh."),
            new ProductSeed("SP0019", "Parka Moss Expedition", "Canvas phủ sáp", "DM004", "TH005", "Nhiều túi, chống gió tốt cho ngày lạnh ngoài trời."),
            new ProductSeed("SP0020", "Áo Len Nordic Rib", "Len gân dày", "DM003", "TH003", "Bề mặt len nổi rõ, lên form ấm và nam tính."),
            new ProductSeed("SP0021", "Áo Khoác Pilot Force", "Twill dày", "DM006", "TH002", "Áo khoác ngắn bo gấu, cảm giác khỏe khoắn và gọn."),
            new ProductSeed("SP0022", "Áo Phao Drift Matte", "Phao mờ chống gió", "DM001", "TH001", "Dáng phao vừa phải, hợp cả đi học lẫn đi làm."),
            new ProductSeed("SP0023", "Heattech Long Sleeve Pro", "Sợi tổng hợp mịn", "DM005", "TH004", "Áo lót dài tay, thoát ẩm nhanh và giữ nhiệt đều."),
            new ProductSeed("SP0024", "Áo Da Urban Ember", "Da mềm phủ bóng nhẹ", "DM002", "TH002", "Tạo điểm nhấn sang hơn khi phối cùng quần tối màu.")
        };
    }

    private static IReadOnlyList<VariantSeed> GetVariantSeeds()
    {
        var variantSpecs = new[]
        {
            new VariantPlan("SP0001", 1290000m, new[] { ("SZM", "BLK", 18), ("SZL", "BLK", 12), ("SZX", "CRM", 7), ("SZ2", "NVY", 5) }),
            new VariantPlan("SP0002", 1490000m, new[] { ("SZM", "BRN", 10), ("SZL", "BRN", 8), ("SZX", "BLK", 6), ("SZ2", "GRY", 4) }),
            new VariantPlan("SP0003", 790000m, new[] { ("SZS", "CRM", 16), ("SZM", "GRY", 14), ("SZL", "GRN", 11), ("SZX", "WHT", 6) }),
            new VariantPlan("SP0004", 1890000m, new[] { ("SZM", "GRY", 9), ("SZL", "CRM", 7), ("SZX", "BLK", 6), ("SZ2", "BRN", 4) }),
            new VariantPlan("SP0005", 690000m, new[] { ("SZM", "GRN", 22), ("SZL", "GRY", 13), ("SZX", "BLK", 8), ("SZ2", "CRM", 5) }),
            new VariantPlan("SP0006", 890000m, new[] { ("SZS", "CRM", 12), ("SZM", "BLK", 15), ("SZL", "GRN", 9), ("SZX", "RUS", 6) }),
            new VariantPlan("SP0007", 990000m, new[] { ("SZS", "GRY", 14), ("SZM", "BLK", 11), ("SZL", "CRM", 8), ("SZX", "NVY", 6) }),
            new VariantPlan("SP0008", 720000m, new[] { ("SZS", "CRM", 17), ("SZM", "GRN", 16), ("SZL", "BRN", 10), ("SZX", "WHT", 6) }),
            new VariantPlan("SP0009", 390000m, new[] { ("SZS", "BLK", 25), ("SZM", "GRY", 20), ("SZL", "CRM", 18), ("SZX", "WHT", 10) }),
            new VariantPlan("SP0010", 1590000m, new[] { ("SZM", "GRN", 13), ("SZL", "BLK", 9), ("SZX", "CRM", 6), ("SZ2", "NVY", 4) }),
            new VariantPlan("SP0011", 680000m, new[] { ("SZS", "CRM", 12), ("SZM", "BRN", 9), ("SZL", "GRY", 11), ("SZX", "WHT", 6) }),
            new VariantPlan("SP0012", 1090000m, new[] { ("SZM", "BLK", 10), ("SZL", "BRN", 8), ("SZX", "GRY", 5), ("SZ2", "NVY", 4) }),
            new VariantPlan("SP0013", 1390000m, new[] { ("SZM", "NVY", 13), ("SZL", "BLK", 11), ("SZX", "CRM", 7), ("SZ2", "GRN", 5) }),
            new VariantPlan("SP0014", 1540000m, new[] { ("SZM", "BLK", 9), ("SZL", "BRN", 8), ("SZX", "GRY", 5), ("SZ2", "RUS", 3) }),
            new VariantPlan("SP0015", 760000m, new[] { ("SZM", "GRY", 18), ("SZL", "CRM", 12), ("SZX", "NVY", 8), ("SZ2", "RUS", 5) }),
            new VariantPlan("SP0016", 1950000m, new[] { ("SZM", "GRY", 8), ("SZL", "BRN", 7), ("SZX", "BLK", 6), ("SZ2", "CRM", 4) }),
            new VariantPlan("SP0017", 420000m, new[] { ("SZS", "WHT", 26), ("SZM", "GRY", 20), ("SZL", "BLK", 15), ("SZX", "CRM", 10) }),
            new VariantPlan("SP0018", 930000m, new[] { ("SZM", "GRN", 14), ("SZL", "BLK", 10), ("SZX", "NVY", 7), ("SZ2", "CRM", 5) }),
            new VariantPlan("SP0019", 1690000m, new[] { ("SZM", "GRN", 9), ("SZL", "BRN", 8), ("SZX", "BLK", 6), ("SZ2", "NVY", 4) }),
            new VariantPlan("SP0020", 810000m, new[] { ("SZS", "CRM", 13), ("SZM", "GRY", 12), ("SZL", "BRN", 9), ("SZX", "RUS", 5) }),
            new VariantPlan("SP0021", 1180000m, new[] { ("SZM", "NVY", 11), ("SZL", "BLK", 10), ("SZX", "GRY", 7), ("SZ2", "BRN", 4) }),
            new VariantPlan("SP0022", 1360000m, new[] { ("SZM", "CRM", 12), ("SZL", "BLK", 10), ("SZX", "GRY", 7), ("SZ2", "NVY", 5) }),
            new VariantPlan("SP0023", 450000m, new[] { ("SZS", "WHT", 24), ("SZM", "CRM", 19), ("SZL", "GRY", 14), ("SZX", "BLK", 9) }),
            new VariantPlan("SP0024", 1580000m, new[] { ("SZM", "BRN", 8), ("SZL", "BLK", 7), ("SZX", "RUS", 5), ("SZ2", "GRY", 3) })
        };

        var result = new List<VariantSeed>();
        var counter = 1;

        foreach (var plan in variantSpecs)
        {
            foreach (var variant in plan.Variants)
            {
                var id = $"CT{counter:0000}";
                var sku = $"{plan.SanPhamID}-{variant.Size}-{variant.Color}";
                var finalPrice = plan.BasePrice + (variant.Size switch
                {
                    "SZX" => 30000m,
                    "SZ2" => 60000m,
                    _ => 0m
                });

                result.Add(new VariantSeed(id, plan.SanPhamID, variant.Size, variant.Color, sku, finalPrice, variant.Stock));
                counter++;
            }
        }

        return result;
    }

    private static string PackAddress(string ward, string street)
    {
        return $"{ward}||{street}";
    }

    private sealed record ProductSeed(
        string SanPhamID,
        string Ten,
        string ChatLieu,
        string DanhMucID,
        string ThuongHieuID,
        string MoTa);

    private sealed record VariantSeed(
        string Id,
        string SanPhamID,
        string KichCoID,
        string MauID,
        string Sku,
        decimal GiaNiemYet,
        int SoLuongTon);

    private sealed record VariantPlan(
        string SanPhamID,
        decimal BasePrice,
        IReadOnlyList<(string Size, string Color, int Stock)> Variants);
}
