using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    public partial class Drop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserListeners");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserListeners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiHash = table.Column<string>(type: "TEXT", nullable: false),
                    ApiId = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserListeners", x => x.Id);
                });
        }
    }
}
