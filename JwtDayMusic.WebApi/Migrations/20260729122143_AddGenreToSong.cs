using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JwtDayMusic.WebApi.Migrations
{
    public partial class AddGenreToSong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Songs");
        }
    }
}
