using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN_70.Migrations
{
    public partial class AddBannerTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    BannerID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    HinhAnhUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LienKetUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    KichHoat = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.BannerID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banners");
        }
    }
}
