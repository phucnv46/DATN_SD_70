CREATE TABLE KichCo (
    KichCoID VARCHAR(10) PRIMARY KEY,
    Ten NVARCHAR(50)
);

CREATE TABLE Mau (
    MauID VARCHAR(10) PRIMARY KEY,
    Ten NVARCHAR(50)
);

CREATE TABLE SanPham (
    SanPhamID VARCHAR(20) PRIMARY KEY,
    Ten NVARCHAR(200),
    MoTa NVARCHAR(MAX)
);

CREATE TABLE ChiTietSanPham (
    ChiTietSanPhamID VARCHAR(20) PRIMARY KEY,
    SanPhamID VARCHAR(20) FOREIGN KEY REFERENCES SanPham(SanPhamID),
    KichCoID VARCHAR(10) FOREIGN KEY REFERENCES KichCo(KichCoID),
    MauID VARCHAR(10) FOREIGN KEY REFERENCES Mau(MauID),
    GiaNiemYet DECIMAL(18,0),
    SoLuongTon INT
);

CREATE TABLE HoaDon (
    HoaDonID VARCHAR(20) PRIMARY KEY,
    TenKhachHang NVARCHAR(100),
    SoDienThoai VARCHAR(15),
    DiaChiGiaoHang NVARCHAR(255),
    NgayTao DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,0),
    TrangThai INT DEFAULT 0
);

CREATE TABLE HoaDonChiTiet (
    HoaDonChiTietID VARCHAR(20) PRIMARY KEY,
    HoaDonID VARCHAR(20) FOREIGN KEY REFERENCES HoaDon(HoaDonID),
    ChiTietSanPhamID VARCHAR(20) FOREIGN KEY REFERENCES ChiTietSanPham(ChiTietSanPhamID),
    SoLuong INT,
    DonGia DECIMAL(18,0),
    ThanhTien DECIMAL(18,0)
);

INSERT INTO KichCo (KichCoID, Ten)
VALUES ('SIZE_M', N'Size M'), ('SIZE_L', N'Size L');

INSERT INTO Mau (MauID, Ten)
VALUES ('MAU_DEN', N'Đen'), ('MAU_BE', N'Be');

INSERT INTO SanPham (SanPhamID, Ten, MoTa)
VALUES ('SP01', N'Áo Phao Lông Vũ Dáng Dài', N'Áo phao siêu ấm, chống nước nhẹ, phù hợp mùa đông giá rét.');

INSERT INTO ChiTietSanPham (ChiTietSanPhamID, SanPhamID, KichCoID, MauID, GiaNiemYet, SoLuongTon)
VALUES
('CTSP01_M_DEN', 'SP01', 'SIZE_M', 'MAU_DEN', 500000, 10),
('CTSP01_L_DEN', 'SP01', 'SIZE_L', 'MAU_DEN', 550000, 5),
('CTSP01_M_BE', 'SP01', 'SIZE_M', 'MAU_BE', 500000, 8);
