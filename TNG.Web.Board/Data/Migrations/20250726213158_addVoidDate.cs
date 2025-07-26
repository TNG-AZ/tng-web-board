using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class addVoidDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositionRoles_Members_MemberId",
                table: "VolunteerPositionRoles");

            migrationBuilder.DropIndex(
                name: "IX_VolunteerPositionRoles_MemberId",
                table: "VolunteerPositionRoles");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "VolunteerPositionRoles");

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedDate",
                table: "EventRsvps",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoidedDate",
                table: "EventRsvps");

            migrationBuilder.AddColumn<Guid>(
                name: "MemberId",
                table: "VolunteerPositionRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerPositionRoles_MemberId",
                table: "VolunteerPositionRoles",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositionRoles_Members_MemberId",
                table: "VolunteerPositionRoles",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
