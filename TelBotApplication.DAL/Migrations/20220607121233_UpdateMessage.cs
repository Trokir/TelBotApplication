using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    public partial class UpdateMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "MessageLoggers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "MessageLoggers");
        }
    }
}
