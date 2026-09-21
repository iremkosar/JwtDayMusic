using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtDayMusic.WebApi.Migrations
{
    public partial class AddRequiredPackageToSong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Songs");

            migrationBuilder.AddColumn<string>(
                name: "RequiredPackage",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredPackage",
                table: "Songs");

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Songs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
