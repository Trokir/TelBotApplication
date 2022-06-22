using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    public partial class InitCallback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TriggerCallBack",
                table: "Anchors",
                newName: "UntilMinutes");

            migrationBuilder.CreateTable(
                name: "AnchorCallbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ButtonText = table.Column<string>(type: "TEXT", nullable: true),
                    ButtonCondition = table.Column<string>(type: "TEXT", nullable: true),
                    AnchorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnchorCallbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnchorCallbacks_Anchors_AnchorId",
                        column: x => x.AnchorId,
                        principalTable: "Anchors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnchorCallbacks_AnchorId",
                table: "AnchorCallbacks",
                column: "AnchorId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnchorCallbacks");

            migrationBuilder.RenameColumn(
                name: "UntilMinutes",
                table: "Anchors",
                newName: "TriggerCallBack");
        }
    }
}
