using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_70.Migrations
{
    /// <inheritdoc />
    public partial class OpDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    DanhMucID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.DanhMucID);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMais",
                columns: table => new
                {
                    KhuyenMaiID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhanTramChietKhau = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    GiaTriToiThieuApDung = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    NgayApDung = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMais", x => x.KhuyenMaiID);
                });

            migrationBuilder.CreateTable(
                name: "KichCos",
                columns: table => new
                {
                    KichCoID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KichCos", x => x.KichCoID);
                });

            migrationBuilder.CreateTable(
                name: "Maus",
                columns: table => new
                {
                    MauID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maus", x => x.MauID);
                });

            migrationBuilder.CreateTable(
                name: "PhuongThucThanhToans",
                columns: table => new
                {
                    PhuongThucThanhToanID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KieuThanhToan = table.Column<int>(type: "int", nullable: false),
                    HinhURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuongThucThanhToans", x => x.PhuongThucThanhToanID);
                });

            migrationBuilder.CreateTable(
                name: "ThuongHieus",
                columns: table => new
                {
                    ThuongHieuID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuongHieus", x => x.ThuongHieuID);
                });

            migrationBuilder.CreateTable(
                name: "VaiTros",
                columns: table => new
                {
                    VaiTroID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTros", x => x.VaiTroID);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    SanPhamID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MucVAT = table.Column<int>(type: "int", nullable: false),
                    ChatLieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThuongHieuID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DanhMucID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.SanPhamID);
                    table.ForeignKey(
                        name: "FK_SanPhams_DanhMucs_DanhMucID",
                        column: x => x.DanhMucID,
                        principalTable: "DanhMucs",
                        principalColumn: "DanhMucID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPhams_ThuongHieus_ThuongHieuID",
                        column: x => x.ThuongHieuID,
                        principalTable: "ThuongHieus",
                        principalColumn: "ThuongHieuID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    TaiKhoanID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VaiTroID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.TaiKhoanID);
                    table.ForeignKey(
                        name: "FK_TaiKhoans_VaiTros_VaiTroID",
                        column: x => x.VaiTroID,
                        principalTable: "VaiTros",
                        principalColumn: "VaiTroID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietSanPhams",
                columns: table => new
                {
                    ChiTietSanPhamID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SoLuongTonKho = table.Column<int>(type: "int", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GiaNiemYet = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    KichCoID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MauID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SanPhamID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietSanPhams", x => x.ChiTietSanPhamID);
                    table.ForeignKey(
                        name: "FK_ChiTietSanPhams_KichCos_KichCoID",
                        column: x => x.KichCoID,
                        principalTable: "KichCos",
                        principalColumn: "KichCoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietSanPhams_Maus_MauID",
                        column: x => x.MauID,
                        principalTable: "Maus",
                        principalColumn: "MauID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietSanPhams_SanPhams_SanPhamID",
                        column: x => x.SanPhamID,
                        principalTable: "SanPhams",
                        principalColumn: "SanPhamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhuyenMaiSanPhams",
                columns: table => new
                {
                    KhuyenMaiID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SanPhamID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhuyenMaiSanPhams", x => new { x.KhuyenMaiID, x.SanPhamID });
                    table.ForeignKey(
                        name: "FK_KhuyenMaiSanPhams_KhuyenMais_KhuyenMaiID",
                        column: x => x.KhuyenMaiID,
                        principalTable: "KhuyenMais",
                        principalColumn: "KhuyenMaiID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KhuyenMaiSanPhams_SanPhams_SanPhamID",
                        column: x => x.SanPhamID,
                        principalTable: "SanPhams",
                        principalColumn: "SanPhamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    GioHangID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaiKhoanID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.GioHangID);
                    table.ForeignKey(
                        name: "FK_GioHangs_TaiKhoans_TaiKhoanID",
                        column: x => x.TaiKhoanID,
                        principalTable: "TaiKhoans",
                        principalColumn: "TaiKhoanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    KhachHangID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: false),
                    TaiKhoanID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.KhachHangID);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_TaiKhoanID",
                        column: x => x.TaiKhoanID,
                        principalTable: "TaiKhoans",
                        principalColumn: "TaiKhoanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhanViens",
                columns: table => new
                {
                    NhanVienID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NgayNhanViec = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaiKhoanID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanViens", x => x.NhanVienID);
                    table.ForeignKey(
                        name: "FK_NhanViens_TaiKhoans_TaiKhoanID",
                        column: x => x.TaiKhoanID,
                        principalTable: "TaiKhoans",
                        principalColumn: "TaiKhoanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietGioHangs",
                columns: table => new
                {
                    ChiTietGioHangID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    GioHangID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChiTietSanPhamID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietGioHangs", x => x.ChiTietGioHangID);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_ChiTietSanPhams_ChiTietSanPhamID",
                        column: x => x.ChiTietSanPhamID,
                        principalTable: "ChiTietSanPhams",
                        principalColumn: "ChiTietSanPhamID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietGioHangs_GioHangs_GioHangID",
                        column: x => x.GioHangID,
                        principalTable: "GioHangs",
                        principalColumn: "GioHangID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaChis",
                columns: table => new
                {
                    DiaChiID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    TenNguoiNhan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoaiNhan = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    TinhThanh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuanHuyen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhuongXa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LaMacDinh = table.Column<bool>(type: "bit", nullable: false),
                    KhachHangID = table.Column<string>(type: "nvarchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaChis", x => x.DiaChiID);
                    table.ForeignKey(
                        name: "FK_DiaChis_KhachHangs_KhachHangID",
                        column: x => x.KhachHangID,
                        principalTable: "KhachHangs",
                        principalColumn: "KhachHangID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    HoaDonID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    TongTienVAT = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TongTienGiamGia = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    LoaiGiaoDich = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    KhachHangID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NhanVienID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaChiID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    KhuyenMaiID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.HoaDonID);
                    table.ForeignKey(
                        name: "FK_HoaDons_DiaChis_DiaChiID",
                        column: x => x.DiaChiID,
                        principalTable: "DiaChis",
                        principalColumn: "DiaChiID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDons_KhachHangs_KhachHangID",
                        column: x => x.KhachHangID,
                        principalTable: "KhachHangs",
                        principalColumn: "KhachHangID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDons_KhuyenMais_KhuyenMaiID",
                        column: x => x.KhuyenMaiID,
                        principalTable: "KhuyenMais",
                        principalColumn: "KhuyenMaiID");
                    table.ForeignKey(
                        name: "FK_HoaDons_NhanViens_NhanVienID",
                        column: x => x.NhanVienID,
                        principalTable: "NhanViens",
                        principalColumn: "NhanVienID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietThanhToans",
                columns: table => new
                {
                    ChiTietThanhToanID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    MaThamChieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThoiGianThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    HoaDonID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PhuongThucThanhToanID = table.Column<string>(type: "nvarchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietThanhToans", x => x.ChiTietThanhToanID);
                    table.ForeignKey(
                        name: "FK_ChiTietThanhToans_HoaDons_HoaDonID",
                        column: x => x.HoaDonID,
                        principalTable: "HoaDons",
                        principalColumn: "HoaDonID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietThanhToans_PhuongThucThanhToans_PhuongThucThanhToanID",
                        column: x => x.PhuongThucThanhToanID,
                        principalTable: "PhuongThucThanhToans",
                        principalColumn: "PhuongThucThanhToanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonChiTiets",
                columns: table => new
                {
                    HoaDonChiTietID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    MucVAT = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TienVAT = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    HoaDonID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChiTietSanPhamID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonChiTiets", x => x.HoaDonChiTietID);
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_ChiTietSanPhams_ChiTietSanPhamID",
                        column: x => x.ChiTietSanPhamID,
                        principalTable: "ChiTietSanPhams",
                        principalColumn: "ChiTietSanPhamID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDonChiTiets_HoaDons_HoaDonID",
                        column: x => x.HoaDonID,
                        principalTable: "HoaDons",
                        principalColumn: "HoaDonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_ChiTietSanPhamID",
                table: "ChiTietGioHangs",
                column: "ChiTietSanPhamID");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietGioHangs_GioHangID",
                table: "ChiTietGioHangs",
                column: "GioHangID");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietSanPhams_KichCoID",
                table: "ChiTietSanPhams",
                column: "KichCoID");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietSanPhams_MauID",
                table: "ChiTietSanPhams",
                column: "MauID");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietSanPhams_SanPhamID",
                table: "ChiTietSanPhams",
                column: "SanPhamID");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietThanhToans_HoaDonID",
                table: "ChiTietThanhToans",
                column: "HoaDonID");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietThanhToans_PhuongThucThanhToanID",
                table: "ChiTietThanhToans",
                column: "PhuongThucThanhToanID");

            migrationBuilder.CreateIndex(
                name: "IX_DiaChis_KhachHangID",
                table: "DiaChis",
                column: "KhachHangID");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_TaiKhoanID",
                table: "GioHangs",
                column: "TaiKhoanID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_ChiTietSanPhamID",
                table: "HoaDonChiTiets",
                column: "ChiTietSanPhamID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonChiTiets_HoaDonID",
                table: "HoaDonChiTiets",
                column: "HoaDonID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_DiaChiID",
                table: "HoaDons",
                column: "DiaChiID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_KhachHangID",
                table: "HoaDons",
                column: "KhachHangID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_KhuyenMaiID",
                table: "HoaDons",
                column: "KhuyenMaiID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_NhanVienID",
                table: "HoaDons",
                column: "NhanVienID");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHangs_TaiKhoanID",
                table: "KhachHangs",
                column: "TaiKhoanID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMaiSanPhams_SanPhamID",
                table: "KhuyenMaiSanPhams",
                column: "SanPhamID");

            migrationBuilder.CreateIndex(
                name: "IX_NhanViens_TaiKhoanID",
                table: "NhanViens",
                column: "TaiKhoanID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_DanhMucID",
                table: "SanPhams",
                column: "DanhMucID");

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_ThuongHieuID",
                table: "SanPhams",
                column: "ThuongHieuID");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_VaiTroID",
                table: "TaiKhoans",
                column: "VaiTroID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietGioHangs");

            migrationBuilder.DropTable(
                name: "ChiTietThanhToans");

            migrationBuilder.DropTable(
                name: "HoaDonChiTiets");

            migrationBuilder.DropTable(
                name: "KhuyenMaiSanPhams");

            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "PhuongThucThanhToans");

            migrationBuilder.DropTable(
                name: "ChiTietSanPhams");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "KichCos");

            migrationBuilder.DropTable(
                name: "Maus");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "DiaChis");

            migrationBuilder.DropTable(
                name: "KhuyenMais");

            migrationBuilder.DropTable(
                name: "NhanViens");

            migrationBuilder.DropTable(
                name: "DanhMucs");

            migrationBuilder.DropTable(
                name: "ThuongHieus");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "VaiTros");
        }
    }
}
