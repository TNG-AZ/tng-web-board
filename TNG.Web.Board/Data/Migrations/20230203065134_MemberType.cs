using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class MemberType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberType",
                table: "Members",
                type: "int",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemberType",
                table: "Members");
        }
    }
}
