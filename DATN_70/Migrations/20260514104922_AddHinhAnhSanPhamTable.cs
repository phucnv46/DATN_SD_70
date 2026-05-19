using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_70.Migrations
{
    /// <inheritdoc />
    public partial class AddHinhAnhSanPhamTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HinhAnhSanPham",
                columns: table => new
                {
                    HinhAnhID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    SanPhamID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MauID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ChiTietThanhToanID = table.Column<string>(type: "nvarchar(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhSanPham", x => x.HinhAnhID);
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPham_ChiTietThanhToans_ChiTietThanhToanID",
                        column: x => x.ChiTietThanhToanID,
                        principalTable: "ChiTietThanhToans",
                        principalColumn: "ChiTietThanhToanID");
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPham_Maus_MauID",
                        column: x => x.MauID,
                        principalTable: "Maus",
                        principalColumn: "MauID");
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPham_SanPhams_SanPhamID",
                        column: x => x.SanPhamID,
                        principalTable: "SanPhams",
                        principalColumn: "SanPhamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPham_ChiTietThanhToanID",
                table: "HinhAnhSanPham",
                column: "ChiTietThanhToanID");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPham_MauID",
                table: "HinhAnhSanPham",
                column: "MauID");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPham_SanPhamID",
                table: "HinhAnhSanPham",
                column: "SanPhamID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HinhAnhSanPham");
        }
    }
}
