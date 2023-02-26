using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class moreFetish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberFetish_Fetish_FetishId",
                table: "MemberFetish");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberFetish_Members_MemberId",
                table: "MemberFetish");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberFetish",
                table: "MemberFetish");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fetish",
                table: "Fetish");

            migrationBuilder.RenameTable(
                name: "MemberFetish",
                newName: "MembersFetishes");

            migrationBuilder.RenameTable(
                name: "Fetish",
                newName: "Fetishes");

            migrationBuilder.RenameIndex(
                name: "IX_MemberFetish_MemberId",
                table: "MembersFetishes",
                newName: "IX_MembersFetishes_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MemberFetish_FetishId",
                table: "MembersFetishes",
                newName: "IX_MembersFetishes_FetishId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembersFetishes",
                table: "MembersFetishes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fetishes",
                table: "Fetishes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MembersFetishes_Fetishes_FetishId",
                table: "MembersFetishes",
                column: "FetishId",
                principalTable: "Fetishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembersFetishes_Members_MemberId",
                table: "MembersFetishes",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembersFetishes_Fetishes_FetishId",
                table: "MembersFetishes");

            migrationBuilder.DropForeignKey(
                name: "FK_MembersFetishes_Members_MemberId",
                table: "MembersFetishes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembersFetishes",
                table: "MembersFetishes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Fetishes",
                table: "Fetishes");

            migrationBuilder.RenameTable(
                name: "MembersFetishes",
                newName: "MemberFetish");

            migrationBuilder.RenameTable(
                name: "Fetishes",
                newName: "Fetish");

            migrationBuilder.RenameIndex(
                name: "IX_MembersFetishes_MemberId",
                table: "MemberFetish",
                newName: "IX_MemberFetish_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MembersFetishes_FetishId",
                table: "MemberFetish",
                newName: "IX_MemberFetish_FetishId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberFetish",
                table: "MemberFetish",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Fetish",
                table: "Fetish",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberFetish_Fetish_FetishId",
                table: "MemberFetish",
                column: "FetishId",
                principalTable: "Fetish",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberFetish_Members_MemberId",
                table: "MemberFetish",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
