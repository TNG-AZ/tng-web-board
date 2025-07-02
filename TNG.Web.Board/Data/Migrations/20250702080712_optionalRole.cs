using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class optionalRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositions_VolunteerPositionRoles_RequiredRoleId",
                table: "VolunteerPositions");

            migrationBuilder.AlterColumn<int>(
                name: "RequiredRoleId",
                table: "VolunteerPositions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositions_VolunteerPositionRoles_RequiredRoleId",
                table: "VolunteerPositions",
                column: "RequiredRoleId",
                principalTable: "VolunteerPositionRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositions_VolunteerPositionRoles_RequiredRoleId",
                table: "VolunteerPositions");

            migrationBuilder.AlterColumn<int>(
                name: "RequiredRoleId",
                table: "VolunteerPositions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositions_VolunteerPositionRoles_RequiredRoleId",
                table: "VolunteerPositions",
                column: "RequiredRoleId",
                principalTable: "VolunteerPositionRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
