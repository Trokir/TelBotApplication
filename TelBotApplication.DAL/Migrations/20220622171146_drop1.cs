using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    public partial class drop1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anchors_Groups_GroupId1",
                table: "Anchors");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageLoggers_Groups_GroupId",
                table: "MessageLoggers");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_MessageLoggers_GroupId",
                table: "MessageLoggers");

            migrationBuilder.DropIndex(
                name: "IX_Anchors_GroupId1",
                table: "Anchors");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "MessageLoggers");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Anchors");

            migrationBuilder.DropColumn(
                name: "GroupId1",
                table: "Anchors");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "MessageLoggers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "GroupId",
                table: "Anchors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "GroupId1",
                table: "Anchors",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageLoggers_GroupId",
                table: "MessageLoggers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Anchors_GroupId1",
                table: "Anchors",
                column: "GroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_GroupId",
                table: "Admins",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Anchors_Groups_GroupId1",
                table: "Anchors",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLoggers_Groups_GroupId",
                table: "MessageLoggers",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
