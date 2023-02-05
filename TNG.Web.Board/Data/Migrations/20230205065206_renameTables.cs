using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class renameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipNotes");

            migrationBuilder.DropTable(
                name: "MembershipOrientations");

            migrationBuilder.CreateTable(
                name: "MembershipNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipNote_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembershipOrientation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipOrientation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipOrientation_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipNote_DateAdded",
                table: "MembershipNote",
                column: "DateAdded",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipNote_MemberId",
                table: "MembershipNote",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipOrientation_DateReceived",
                table: "MembershipOrientation",
                column: "DateReceived",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipOrientation_MemberId",
                table: "MembershipOrientation",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipNote");

            migrationBuilder.DropTable(
                name: "MembershipOrientation");

            migrationBuilder.CreateTable(
                name: "MembershipNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipNotes_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembershipOrientations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipOrientations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipOrientations_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipNotes_DateAdded",
                table: "MembershipNotes",
                column: "DateAdded",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipNotes_MemberId",
                table: "MembershipNotes",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipOrientations_DateReceived",
                table: "MembershipOrientations",
                column: "DateReceived",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipOrientations_MemberId",
                table: "MembershipOrientations",
                column: "MemberId");
        }
    }
}
