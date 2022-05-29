using Microsoft.EntityFrameworkCore.Migrations;

namespace TelBotApplication.DAL.Migrations
{
    public partial class AddDescriptoinField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BotCallers",
                type: "TEXT",
                maxLength: 24,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "BotCallers");
        }
    }
}
