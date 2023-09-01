using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class plusOnes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventRsvpPlusOnes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlusOneMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRsvpPlusOnes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRsvpPlusOnes_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction,
                        onUpdate: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_EventRsvpPlusOnes_Members_PlusOneMemberId",
                        column: x => x.PlusOneMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction,
                        onUpdate: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvpPlusOnes_MemberId",
                table: "EventRsvpPlusOnes",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRsvpPlusOnes_PlusOneMemberId",
                table: "EventRsvpPlusOnes",
                column: "PlusOneMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRsvpPlusOnes");
        }
    }
}
