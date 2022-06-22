using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    public partial class Cha : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnchorCallbacks_Anchors_AnchorId",
                table: "AnchorCallbacks");

            migrationBuilder.DropIndex(
                name: "IX_AnchorCallbacks_AnchorId",
                table: "AnchorCallbacks");

            migrationBuilder.DropColumn(
                name: "AnchorId",
                table: "AnchorCallbacks");

            migrationBuilder.AddColumn<int>(
                name: "AnchorCallbackId",
                table: "Anchors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Anchors_AnchorCallbackId",
                table: "Anchors",
                column: "AnchorCallbackId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Anchors_AnchorCallbacks_AnchorCallbackId",
                table: "Anchors",
                column: "AnchorCallbackId",
                principalTable: "AnchorCallbacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anchors_AnchorCallbacks_AnchorCallbackId",
                table: "Anchors");

            migrationBuilder.DropIndex(
                name: "IX_Anchors_AnchorCallbackId",
                table: "Anchors");

            migrationBuilder.DropColumn(
                name: "AnchorCallbackId",
                table: "Anchors");

            migrationBuilder.AddColumn<int>(
                name: "AnchorId",
                table: "AnchorCallbacks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AnchorCallbacks_AnchorId",
                table: "AnchorCallbacks",
                column: "AnchorId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AnchorCallbacks_Anchors_AnchorId",
                table: "AnchorCallbacks",
                column: "AnchorId",
                principalTable: "Anchors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
