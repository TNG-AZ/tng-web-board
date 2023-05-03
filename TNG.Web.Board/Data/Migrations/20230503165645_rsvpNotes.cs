using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class rsvpNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "EventRsvps",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "EventRsvps");
        }
    }
}
