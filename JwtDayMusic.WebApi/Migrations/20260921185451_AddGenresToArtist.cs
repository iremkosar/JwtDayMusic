using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtDayMusic.WebApi.Migrations
{
    public partial class AddGenresToArtist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genres",
                table: "Artists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genres",
                table: "Artists");
        }
    }
}
