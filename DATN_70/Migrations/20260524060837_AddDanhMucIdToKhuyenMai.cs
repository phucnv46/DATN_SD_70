using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_70.Migrations
{
    /// <inheritdoc />
    public partial class AddDanhMucIdToKhuyenMai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DanhMucID",
                table: "KhuyenMais",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhuyenMais_DanhMucID",
                table: "KhuyenMais",
                column: "DanhMucID");

            migrationBuilder.AddForeignKey(
                name: "FK_KhuyenMais_DanhMucs_DanhMucID",
                table: "KhuyenMais",
                column: "DanhMucID",
                principalTable: "DanhMucs",
                principalColumn: "DanhMucID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KhuyenMais_DanhMucs_DanhMucID",
                table: "KhuyenMais");

            migrationBuilder.DropIndex(
                name: "IX_KhuyenMais_DanhMucID",
                table: "KhuyenMais");

            migrationBuilder.DropColumn(
                name: "DanhMucID",
                table: "KhuyenMais");
        }
    }
}
