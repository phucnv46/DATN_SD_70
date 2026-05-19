using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_70.Migrations
{
    /// <inheritdoc />
    public partial class FixSaiLinkHinhAnhSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HinhAnhSanPham_ChiTietThanhToans_ChiTietThanhToanID",
                table: "HinhAnhSanPham");

            migrationBuilder.DropForeignKey(
                name: "FK_HinhAnhSanPham_Maus_MauID",
                table: "HinhAnhSanPham");

            migrationBuilder.DropForeignKey(
                name: "FK_HinhAnhSanPham_SanPhams_SanPhamID",
                table: "HinhAnhSanPham");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HinhAnhSanPham",
                table: "HinhAnhSanPham");

            migrationBuilder.DropIndex(
                name: "IX_HinhAnhSanPham_ChiTietThanhToanID",
                table: "HinhAnhSanPham");

            migrationBuilder.DropColumn(
                name: "ChiTietThanhToanID",
                table: "HinhAnhSanPham");

            migrationBuilder.RenameTable(
                name: "HinhAnhSanPham",
                newName: "HinhAnhSanPhams");

            migrationBuilder.RenameIndex(
                name: "IX_HinhAnhSanPham_SanPhamID",
                table: "HinhAnhSanPhams",
                newName: "IX_HinhAnhSanPhams_SanPhamID");

            migrationBuilder.RenameIndex(
                name: "IX_HinhAnhSanPham_MauID",
                table: "HinhAnhSanPhams",
                newName: "IX_HinhAnhSanPhams_MauID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HinhAnhSanPhams",
                table: "HinhAnhSanPhams",
                column: "HinhAnhID");

            migrationBuilder.AddForeignKey(
                name: "FK_HinhAnhSanPhams_Maus_MauID",
                table: "HinhAnhSanPhams",
                column: "MauID",
                principalTable: "Maus",
                principalColumn: "MauID");

            migrationBuilder.AddForeignKey(
                name: "FK_HinhAnhSanPhams_SanPhams_SanPhamID",
                table: "HinhAnhSanPhams",
                column: "SanPhamID",
                principalTable: "SanPhams",
                principalColumn: "SanPhamID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HinhAnhSanPhams_Maus_MauID",
                table: "HinhAnhSanPhams");

            migrationBuilder.DropForeignKey(
                name: "FK_HinhAnhSanPhams_SanPhams_SanPhamID",
                table: "HinhAnhSanPhams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HinhAnhSanPhams",
                table: "HinhAnhSanPhams");

            migrationBuilder.RenameTable(
                name: "HinhAnhSanPhams",
                newName: "HinhAnhSanPham");

            migrationBuilder.RenameIndex(
                name: "IX_HinhAnhSanPhams_SanPhamID",
                table: "HinhAnhSanPham",
                newName: "IX_HinhAnhSanPham_SanPhamID");

            migrationBuilder.RenameIndex(
                name: "IX_HinhAnhSanPhams_MauID",
                table: "HinhAnhSanPham",
                newName: "IX_HinhAnhSanPham_MauID");

            migrationBuilder.AddColumn<string>(
                name: "ChiTietThanhToanID",
                table: "HinhAnhSanPham",
                type: "nvarchar(36)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_HinhAnhSanPham",
                table: "HinhAnhSanPham",
                column: "HinhAnhID");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPham_ChiTietThanhToanID",
                table: "HinhAnhSanPham",
                column: "ChiTietThanhToanID");

            migrationBuilder.AddForeignKey(
                name: "FK_HinhAnhSanPham_ChiTietThanhToans_ChiTietThanhToanID",
                table: "HinhAnhSanPham",
                column: "ChiTietThanhToanID",
                principalTable: "ChiTietThanhToans",
                principalColumn: "ChiTietThanhToanID");

            migrationBuilder.AddForeignKey(
                name: "FK_HinhAnhSanPham_Maus_MauID",
                table: "HinhAnhSanPham",
                column: "MauID",
                principalTable: "Maus",
                principalColumn: "MauID");

            migrationBuilder.AddForeignKey(
                name: "FK_HinhAnhSanPham_SanPhams_SanPhamID",
                table: "HinhAnhSanPham",
                column: "SanPhamID",
                principalTable: "SanPhams",
                principalColumn: "SanPhamID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
