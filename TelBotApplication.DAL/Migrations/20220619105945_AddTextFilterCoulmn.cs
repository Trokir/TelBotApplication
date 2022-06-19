using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    public partial class AddTextFilterCoulmn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "TextFilters",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "TextFilters");
        }
    }
}
