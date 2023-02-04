using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorNotesIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MembershipNotes_MemberId_DateAdded",
                table: "MembershipNotes");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipNotes_DateAdded",
                table: "MembershipNotes",
                column: "DateAdded",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipNotes_MemberId",
                table: "MembershipNotes",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MembershipNotes_DateAdded",
                table: "MembershipNotes");

            migrationBuilder.DropIndex(
                name: "IX_MembershipNotes_MemberId",
                table: "MembershipNotes");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipNotes_MemberId_DateAdded",
                table: "MembershipNotes",
                columns: new[] { "MemberId", "DateAdded" },
                descending: new[] { false, true });
        }
    }
}
