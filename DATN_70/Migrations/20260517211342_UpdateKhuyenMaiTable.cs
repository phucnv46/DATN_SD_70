using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_70.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKhuyenMaiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhanTramChietKhau",
                table: "KhuyenMais");

            migrationBuilder.AddColumn<decimal>(
                name: "GiaTriGiam",
                table: "KhuyenMais",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GiamToiDa",
                table: "KhuyenMais",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LoaiGiamGia",
                table: "KhuyenMais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaCode",
                table: "KhuyenMais",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongDaDung",
                table: "KhuyenMais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongToiDa",
                table: "KhuyenMais",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiaTriGiam",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "GiamToiDa",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "LoaiGiamGia",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "MaCode",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "SoLuongDaDung",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "SoLuongToiDa",
                table: "KhuyenMais");

            migrationBuilder.AddColumn<decimal>(
                name: "PhanTramChietKhau",
                table: "KhuyenMais",
                type: "decimal(18,0)",
                precision: 18,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
