using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_70.Migrations
{
    /// <inheritdoc />
    public partial class AddRMA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "phieuDoiTras",
                columns: table => new
                {
                    PhieuDoiTraID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    HoaDonID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    TongTienHoan = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    GhiChuAdmin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phieuDoiTras", x => x.PhieuDoiTraID);
                    table.ForeignKey(
                        name: "FK_phieuDoiTras_HoaDons_HoaDonID",
                        column: x => x.HoaDonID,
                        principalTable: "HoaDons",
                        principalColumn: "HoaDonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chiTietDoiTras",
                columns: table => new
                {
                    ChiTietDoiTraID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    PhieuDoiTraID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ChiTietSanPhamID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SoLuongTra = table.Column<int>(type: "int", nullable: false),
                    GiaTriHoanLai = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    LyDo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chiTietDoiTras", x => x.ChiTietDoiTraID);
                    table.ForeignKey(
                        name: "FK_chiTietDoiTras_ChiTietSanPhams_ChiTietSanPhamID",
                        column: x => x.ChiTietSanPhamID,
                        principalTable: "ChiTietSanPhams",
                        principalColumn: "ChiTietSanPhamID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chiTietDoiTras_phieuDoiTras_PhieuDoiTraID",
                        column: x => x.PhieuDoiTraID,
                        principalTable: "phieuDoiTras",
                        principalColumn: "PhieuDoiTraID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chiTietDoiTras_ChiTietSanPhamID",
                table: "chiTietDoiTras",
                column: "ChiTietSanPhamID");

            migrationBuilder.CreateIndex(
                name: "IX_chiTietDoiTras_PhieuDoiTraID",
                table: "chiTietDoiTras",
                column: "PhieuDoiTraID");

            migrationBuilder.CreateIndex(
                name: "IX_phieuDoiTras_HoaDonID",
                table: "phieuDoiTras",
                column: "HoaDonID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chiTietDoiTras");

            migrationBuilder.DropTable(
                name: "phieuDoiTras");
        }
    }
}
