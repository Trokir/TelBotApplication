using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    public partial class Chaq11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnchorCallBack",
                table: "Anchors",
                newName: "AnchorCallBackType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnchorCallBackType",
                table: "Anchors",
                newName: "AnchorCallBack");
        }
    }
}
