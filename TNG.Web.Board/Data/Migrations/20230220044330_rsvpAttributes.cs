using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class rsvpAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "EventRsvps",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Attended",
                table: "EventRsvps",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "EventRsvps",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "EventRsvps");

            migrationBuilder.DropColumn(
                name: "Attended",
                table: "EventRsvps");

            migrationBuilder.DropColumn(
                name: "Paid",
                table: "EventRsvps");
        }
    }
}
