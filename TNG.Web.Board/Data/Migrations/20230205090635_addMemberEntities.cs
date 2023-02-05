using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class addMemberEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipNote_Members_MemberId",
                table: "MembershipNote");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipOrientation_Members_MemberId",
                table: "MembershipOrientation");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPayments_Members_MemberId",
                table: "MembershipPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSuspensions_Members_MemberId",
                table: "MembershipSuspensions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembershipSuspensions",
                table: "MembershipSuspensions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembershipPayments",
                table: "MembershipPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembershipOrientation",
                table: "MembershipOrientation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MembershipNote",
                table: "MembershipNote");

            migrationBuilder.RenameTable(
                name: "MembershipSuspensions",
                newName: "MemberSuspensions");

            migrationBuilder.RenameTable(
                name: "MembershipPayments",
                newName: "MemberDuesPayments");

            migrationBuilder.RenameTable(
                name: "MembershipOrientation",
                newName: "MemberOrientations");

            migrationBuilder.RenameTable(
                name: "MembershipNote",
                newName: "MemberNotes");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipSuspensions_MemberId",
                table: "MemberSuspensions",
                newName: "IX_MemberSuspensions_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipPayments_PaidOn",
                table: "MemberDuesPayments",
                newName: "IX_MemberDuesPayments_PaidOn");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipPayments_MemberId",
                table: "MemberDuesPayments",
                newName: "IX_MemberDuesPayments_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipOrientation_MemberId",
                table: "MemberOrientations",
                newName: "IX_MemberOrientations_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipOrientation_DateReceived",
                table: "MemberOrientations",
                newName: "IX_MemberOrientations_DateReceived");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipNote_MemberId",
                table: "MemberNotes",
                newName: "IX_MemberNotes_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MembershipNote_DateAdded",
                table: "MemberNotes",
                newName: "IX_MemberNotes_DateAdded");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberSuspensions",
                table: "MemberSuspensions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberDuesPayments",
                table: "MemberDuesPayments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberOrientations",
                table: "MemberOrientations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberNotes",
                table: "MemberNotes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberDuesPayments_Members_MemberId",
                table: "MemberDuesPayments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberNotes_Members_MemberId",
                table: "MemberNotes",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberOrientations_Members_MemberId",
                table: "MemberOrientations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MemberSuspensions_Members_MemberId",
                table: "MemberSuspensions",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberDuesPayments_Members_MemberId",
                table: "MemberDuesPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberNotes_Members_MemberId",
                table: "MemberNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberOrientations_Members_MemberId",
                table: "MemberOrientations");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberSuspensions_Members_MemberId",
                table: "MemberSuspensions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberSuspensions",
                table: "MemberSuspensions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberOrientations",
                table: "MemberOrientations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberNotes",
                table: "MemberNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberDuesPayments",
                table: "MemberDuesPayments");

            migrationBuilder.RenameTable(
                name: "MemberSuspensions",
                newName: "MembershipSuspensions");

            migrationBuilder.RenameTable(
                name: "MemberOrientations",
                newName: "MembershipOrientation");

            migrationBuilder.RenameTable(
                name: "MemberNotes",
                newName: "MembershipNote");

            migrationBuilder.RenameTable(
                name: "MemberDuesPayments",
                newName: "MembershipPayments");

            migrationBuilder.RenameIndex(
                name: "IX_MemberSuspensions_MemberId",
                table: "MembershipSuspensions",
                newName: "IX_MembershipSuspensions_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MemberOrientations_MemberId",
                table: "MembershipOrientation",
                newName: "IX_MembershipOrientation_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MemberOrientations_DateReceived",
                table: "MembershipOrientation",
                newName: "IX_MembershipOrientation_DateReceived");

            migrationBuilder.RenameIndex(
                name: "IX_MemberNotes_MemberId",
                table: "MembershipNote",
                newName: "IX_MembershipNote_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_MemberNotes_DateAdded",
                table: "MembershipNote",
                newName: "IX_MembershipNote_DateAdded");

            migrationBuilder.RenameIndex(
                name: "IX_MemberDuesPayments_PaidOn",
                table: "MembershipPayments",
                newName: "IX_MembershipPayments_PaidOn");

            migrationBuilder.RenameIndex(
                name: "IX_MemberDuesPayments_MemberId",
                table: "MembershipPayments",
                newName: "IX_MembershipPayments_MemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembershipSuspensions",
                table: "MembershipSuspensions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembershipOrientation",
                table: "MembershipOrientation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembershipNote",
                table: "MembershipNote",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MembershipPayments",
                table: "MembershipPayments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipNote_Members_MemberId",
                table: "MembershipNote",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipOrientation_Members_MemberId",
                table: "MembershipOrientation",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPayments_Members_MemberId",
                table: "MembershipPayments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSuspensions_Members_MemberId",
                table: "MembershipSuspensions",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
