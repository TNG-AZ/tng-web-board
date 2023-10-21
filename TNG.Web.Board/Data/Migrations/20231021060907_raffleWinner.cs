using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class raffleWinner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WinnerMemberId",
                table: "Raffles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Raffles_WinnerMemberId",
                table: "Raffles",
                column: "WinnerMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Raffles_Members_WinnerMemberId",
                table: "Raffles",
                column: "WinnerMemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Raffles_Members_WinnerMemberId",
                table: "Raffles");

            migrationBuilder.DropIndex(
                name: "IX_Raffles_WinnerMemberId",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "WinnerMemberId",
                table: "Raffles");
        }
    }
}
