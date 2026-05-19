/*
    Script tao DB clean cho du an DATN_70.
    Doi ten DB neu can o bien @DatabaseName truoc khi chay.
*/

SET NOCOUNT ON;

DECLARE @DatabaseName SYSNAME = N'WinterFashion_SD70';
DECLARE @Sql NVARCHAR(MAX);

IF DB_ID(@DatabaseName) IS NOT NULL
BEGIN
    SET @Sql = N'
        ALTER DATABASE [' + @DatabaseName + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        DROP DATABASE [' + @DatabaseName + N'];';
    EXEC sp_executesql @Sql;
END;

SET @Sql = N'CREATE DATABASE [' + @DatabaseName + N'];';
EXEC sp_executesql @Sql;

SET @Sql = N'USE [' + @DatabaseName + N'];

CREATE TABLE dbo.VaiTro (
    VaiTroID NVARCHAR(450) NOT NULL CONSTRAINT PK_VaiTro PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL
);

CREATE TABLE dbo.DanhMuc (
    DanhMucID NVARCHAR(450) NOT NULL CONSTRAINT PK_DanhMuc PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL
);

CREATE TABLE dbo.ThuongHieu (
    ThuongHieuID NVARCHAR(450) NOT NULL CONSTRAINT PK_ThuongHieu PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    LogoURL NVARCHAR(MAX) NOT NULL,
    MoTa NVARCHAR(MAX) NOT NULL
);

CREATE TABLE dbo.KichCo (
    KichCoID NVARCHAR(450) NOT NULL CONSTRAINT PK_KichCo PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    MoTa NVARCHAR(MAX) NOT NULL CONSTRAINT DF_KichCo_MoTa DEFAULT N''''
);

CREATE TABLE dbo.Mau (
    MauID NVARCHAR(450) NOT NULL CONSTRAINT PK_Mau PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL
);

CREATE TABLE dbo.TaiKhoan (
    TaiKhoanID NVARCHAR(450) NOT NULL CONSTRAINT PK_TaiKhoan PRIMARY KEY,
    Email NVARCHAR(MAX) NOT NULL,
    MatKhau NVARCHAR(MAX) NOT NULL,
    TrangThai NVARCHAR(MAX) NOT NULL,
    VaiTroID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.KhachHang (
    KhachHangID NVARCHAR(450) NOT NULL CONSTRAINT PK_KhachHang PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    SoDienThoai NVARCHAR(MAX) NOT NULL,
    GioiTinh INT NOT NULL,
    DiaChi NVARCHAR(MAX) NOT NULL,
    DiemTichLuy INT NOT NULL CONSTRAINT DF_KhachHang_DiemTichLuy DEFAULT 0,
    TaiKhoanID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.NhanVien (
    NhanVienID NVARCHAR(450) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    SoDienThoai NVARCHAR(MAX) NOT NULL,
    GioiTinh INT NOT NULL,
    DiaChi NVARCHAR(MAX) NOT NULL,
    NgayNhanViec DATETIME2 NOT NULL,
    TaiKhoanID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.DiaChi (
    DiaChiID NVARCHAR(450) NOT NULL CONSTRAINT PK_DiaChi PRIMARY KEY,
    TenNguoiNhan NVARCHAR(MAX) NOT NULL,
    SoDienThoaiNhan NVARCHAR(MAX) NOT NULL,
    TinhThanh NVARCHAR(MAX) NOT NULL,
    QuanHuyen NVARCHAR(MAX) NOT NULL,
    PhuongXa NVARCHAR(MAX) NOT NULL,
    LaMacDinh BIT NOT NULL CONSTRAINT DF_DiaChi_LaMacDinh DEFAULT 0,
    KhachHangID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.SanPham (
    SanPhamID NVARCHAR(450) NOT NULL CONSTRAINT PK_SanPham PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    MucVAT FLOAT NOT NULL CONSTRAINT DF_SanPham_MucVAT DEFAULT 0,
    ChatLieu NVARCHAR(MAX) NOT NULL CONSTRAINT DF_SanPham_ChatLieu DEFAULT N'''',
    MoTa NVARCHAR(MAX) NOT NULL CONSTRAINT DF_SanPham_MoTa DEFAULT N'''',
    ThuongHieuID NVARCHAR(450) NOT NULL,
    DanhMucID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.ChiTietSanPham (
    ChiTietSanPhamID NVARCHAR(450) NOT NULL CONSTRAINT PK_ChiTietSanPham PRIMARY KEY,
    SoLuongTon INT NOT NULL CONSTRAINT DF_ChiTietSanPham_SoLuongTon DEFAULT 0,
    SKU NVARCHAR(MAX) NOT NULL CONSTRAINT DF_ChiTietSanPham_SKU DEFAULT N'''',
    GiaNiemYet DECIMAL(18,2) NOT NULL CONSTRAINT DF_ChiTietSanPham_GiaNiemYet DEFAULT 0,
    KichCoID NVARCHAR(450) NOT NULL,
    MauID NVARCHAR(450) NOT NULL,
    SanPhamID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.GioHang (
    GioHangID NVARCHAR(450) NOT NULL CONSTRAINT PK_GioHang PRIMARY KEY,
    NgayTao DATETIME2 NOT NULL,
    TaiKhoanID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.ChiTietGioHang (
    ChiTietGioHangID NVARCHAR(450) NOT NULL CONSTRAINT PK_ChiTietGioHang PRIMARY KEY,
    SoLuong INT NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL,
    GioHangID NVARCHAR(450) NOT NULL,
    ChiTietSanPhamID NVARCHAR(450) NOT NULL
);

CREATE TABLE dbo.HoaDon (
    HoaDonID NVARCHAR(450) NOT NULL CONSTRAINT PK_HoaDon PRIMARY KEY,
    TenKhachHang NVARCHAR(100) NOT NULL,
    SoDienThoai NVARCHAR(20) NOT NULL,
    DiaChiGiaoHang NVARCHAR(255) NOT NULL,
    NgayTao DATETIME2 NOT NULL CONSTRAINT DF_HoaDon_NgayTao DEFAULT SYSUTCDATETIME(),
    TongTien DECIMAL(18,2) NOT NULL,
    TrangThai INT NOT NULL CONSTRAINT DF_HoaDon_TrangThai DEFAULT 0
);

CREATE TABLE dbo.HoaDonChiTiet (
    HoaDonChiTietID NVARCHAR(450) NOT NULL CONSTRAINT PK_HoaDonChiTiet PRIMARY KEY,
    HoaDonID NVARCHAR(450) NOT NULL,
    ChiTietSanPhamID NVARCHAR(450) NOT NULL,
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL
);

CREATE TABLE dbo.KhuyenMai (
    KhuyenMaiID NVARCHAR(450) NOT NULL CONSTRAINT PK_KhuyenMai PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    PhanTramChietKhau DECIMAL(18,2) NOT NULL,
    GiaTriToiThieuApDung DECIMAL(18,2) NOT NULL,
    NgayApDung DATETIME2 NOT NULL,
    NgayKetThuc DATETIME2 NOT NULL,
    MoTa NVARCHAR(MAX) NOT NULL,
    TrangThai INT NOT NULL
);

CREATE TABLE dbo.KhuyenMaiSanPham (
    KhuyenMaiID NVARCHAR(450) NOT NULL,
    SanPhamID NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_KhuyenMaiSanPham PRIMARY KEY (KhuyenMaiID, SanPhamID)
);

CREATE TABLE dbo.PhuongThucThanhToan (
    PhuongThucThanhToanID NVARCHAR(450) NOT NULL CONSTRAINT PK_PhuongThucThanhToan PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    KieuThanhToan NVARCHAR(MAX) NOT NULL,
    HinhURL NVARCHAR(MAX) NOT NULL,
    TrangThai INT NOT NULL
);

CREATE TABLE dbo.ChiTietThanhToan (
    ChiTietThanhToanID NVARCHAR(450) NOT NULL CONSTRAINT PK_ChiTietThanhToan PRIMARY KEY,
    Ten NVARCHAR(MAX) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    MaThamChieu NVARCHAR(MAX) NOT NULL,
    ThoiGianThanhToan DATETIME2 NOT NULL,
    TrangThai INT NOT NULL,
    HoaDonID NVARCHAR(450) NOT NULL,
    PhuongThucThanhToanID NVARCHAR(450) NOT NULL
);

CREATE INDEX IX_TaiKhoan_VaiTroID ON dbo.TaiKhoan (VaiTroID);
CREATE UNIQUE INDEX IX_KhachHang_TaiKhoanID ON dbo.KhachHang (TaiKhoanID);
CREATE UNIQUE INDEX IX_NhanVien_TaiKhoanID ON dbo.NhanVien (TaiKhoanID);
CREATE INDEX IX_DiaChi_KhachHangID ON dbo.DiaChi (KhachHangID);
CREATE INDEX IX_SanPham_ThuongHieuID ON dbo.SanPham (ThuongHieuID);
CREATE INDEX IX_SanPham_DanhMucID ON dbo.SanPham (DanhMucID);
CREATE INDEX IX_ChiTietSanPham_KichCoID ON dbo.ChiTietSanPham (KichCoID);
CREATE INDEX IX_ChiTietSanPham_MauID ON dbo.ChiTietSanPham (MauID);
CREATE INDEX IX_ChiTietSanPham_SanPhamID ON dbo.ChiTietSanPham (SanPhamID);
CREATE UNIQUE INDEX IX_GioHang_TaiKhoanID ON dbo.GioHang (TaiKhoanID);
CREATE INDEX IX_ChiTietGioHang_GioHangID ON dbo.ChiTietGioHang (GioHangID);
CREATE INDEX IX_ChiTietGioHang_ChiTietSanPhamID ON dbo.ChiTietGioHang (ChiTietSanPhamID);
CREATE INDEX IX_HoaDonChiTiet_HoaDonID ON dbo.HoaDonChiTiet (HoaDonID);
CREATE INDEX IX_HoaDonChiTiet_ChiTietSanPhamID ON dbo.HoaDonChiTiet (ChiTietSanPhamID);
CREATE INDEX IX_KhuyenMaiSanPham_SanPhamID ON dbo.KhuyenMaiSanPham (SanPhamID);
CREATE INDEX IX_ChiTietThanhToan_HoaDonID ON dbo.ChiTietThanhToan (HoaDonID);
CREATE INDEX IX_ChiTietThanhToan_PhuongThucThanhToanID ON dbo.ChiTietThanhToan (PhuongThucThanhToanID);

ALTER TABLE dbo.TaiKhoan
ADD CONSTRAINT FK_TaiKhoan_VaiTro_VaiTroID
FOREIGN KEY (VaiTroID) REFERENCES dbo.VaiTro (VaiTroID);

ALTER TABLE dbo.KhachHang
ADD CONSTRAINT FK_KhachHang_TaiKhoan_TaiKhoanID
FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan (TaiKhoanID);

ALTER TABLE dbo.NhanVien
ADD CONSTRAINT FK_NhanVien_TaiKhoan_TaiKhoanID
FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan (TaiKhoanID);

ALTER TABLE dbo.DiaChi
ADD CONSTRAINT FK_DiaChi_KhachHang_KhachHangID
FOREIGN KEY (KhachHangID) REFERENCES dbo.KhachHang (KhachHangID);

ALTER TABLE dbo.SanPham
ADD CONSTRAINT FK_SanPham_ThuongHieu_ThuongHieuID
FOREIGN KEY (ThuongHieuID) REFERENCES dbo.ThuongHieu (ThuongHieuID);

ALTER TABLE dbo.SanPham
ADD CONSTRAINT FK_SanPham_DanhMuc_DanhMucID
FOREIGN KEY (DanhMucID) REFERENCES dbo.DanhMuc (DanhMucID);

ALTER TABLE dbo.ChiTietSanPham
ADD CONSTRAINT FK_ChiTietSanPham_KichCo_KichCoID
FOREIGN KEY (KichCoID) REFERENCES dbo.KichCo (KichCoID);

ALTER TABLE dbo.ChiTietSanPham
ADD CONSTRAINT FK_ChiTietSanPham_Mau_MauID
FOREIGN KEY (MauID) REFERENCES dbo.Mau (MauID);

ALTER TABLE dbo.ChiTietSanPham
ADD CONSTRAINT FK_ChiTietSanPham_SanPham_SanPhamID
FOREIGN KEY (SanPhamID) REFERENCES dbo.SanPham (SanPhamID);

ALTER TABLE dbo.GioHang
ADD CONSTRAINT FK_GioHang_TaiKhoan_TaiKhoanID
FOREIGN KEY (TaiKhoanID) REFERENCES dbo.TaiKhoan (TaiKhoanID);

ALTER TABLE dbo.ChiTietGioHang
ADD CONSTRAINT FK_ChiTietGioHang_GioHang_GioHangID
FOREIGN KEY (GioHangID) REFERENCES dbo.GioHang (GioHangID);

ALTER TABLE dbo.ChiTietGioHang
ADD CONSTRAINT FK_ChiTietGioHang_ChiTietSanPham_ChiTietSanPhamID
FOREIGN KEY (ChiTietSanPhamID) REFERENCES dbo.ChiTietSanPham (ChiTietSanPhamID);

ALTER TABLE dbo.HoaDonChiTiet
ADD CONSTRAINT FK_HoaDonChiTiet_HoaDon_HoaDonID
FOREIGN KEY (HoaDonID) REFERENCES dbo.HoaDon (HoaDonID);

ALTER TABLE dbo.HoaDonChiTiet
ADD CONSTRAINT FK_HoaDonChiTiet_ChiTietSanPham_ChiTietSanPhamID
FOREIGN KEY (ChiTietSanPhamID) REFERENCES dbo.ChiTietSanPham (ChiTietSanPhamID);

ALTER TABLE dbo.KhuyenMaiSanPham
ADD CONSTRAINT FK_KhuyenMaiSanPham_KhuyenMai_KhuyenMaiID
FOREIGN KEY (KhuyenMaiID) REFERENCES dbo.KhuyenMai (KhuyenMaiID);

ALTER TABLE dbo.KhuyenMaiSanPham
ADD CONSTRAINT FK_KhuyenMaiSanPham_SanPham_SanPhamID
FOREIGN KEY (SanPhamID) REFERENCES dbo.SanPham (SanPhamID);

ALTER TABLE dbo.ChiTietThanhToan
ADD CONSTRAINT FK_ChiTietThanhToan_HoaDon_HoaDonID
FOREIGN KEY (HoaDonID) REFERENCES dbo.HoaDon (HoaDonID);

ALTER TABLE dbo.ChiTietThanhToan
ADD CONSTRAINT FK_ChiTietThanhToan_PhuongThucThanhToan_PhuongThucThanhToanID
FOREIGN KEY (PhuongThucThanhToanID) REFERENCES dbo.PhuongThucThanhToan (PhuongThucThanhToanID);';

EXEC sp_executesql @Sql;
