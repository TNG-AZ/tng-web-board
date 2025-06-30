using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class Volunteering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VolunteerPositionRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VolunteerPositionRoleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerPositionRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerPositionRole_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VolunteerPositionRole_VolunteerPositionRole_VolunteerPositionRoleId",
                        column: x => x.VolunteerPositionRoleId,
                        principalTable: "VolunteerPositionRole",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VolunteerPosition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerPosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerPosition_VolunteerPositionRole_RequiredRoleId",
                        column: x => x.RequiredRoleId,
                        principalTable: "VolunteerPositionRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerEventSlot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    NeededCount = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerEventSlot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerEventSlot_VolunteerPosition_PositionId",
                        column: x => x.PositionId,
                        principalTable: "VolunteerPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerSlotMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approval = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerSlotMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerSlotMember_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VolunteerSlotMember_VolunteerEventSlot_SlotId",
                        column: x => x.SlotId,
                        principalTable: "VolunteerEventSlot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerEventSlot_PositionId",
                table: "VolunteerEventSlot",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerPosition_RequiredRoleId",
                table: "VolunteerPosition",
                column: "RequiredRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerPositionRole_MemberId",
                table: "VolunteerPositionRole",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerPositionRole_VolunteerPositionRoleId",
                table: "VolunteerPositionRole",
                column: "VolunteerPositionRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerSlotMember_MemberId",
                table: "VolunteerSlotMember",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerSlotMember_SlotId",
                table: "VolunteerSlotMember",
                column: "SlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VolunteerSlotMember");

            migrationBuilder.DropTable(
                name: "VolunteerEventSlot");

            migrationBuilder.DropTable(
                name: "VolunteerPosition");

            migrationBuilder.DropTable(
                name: "VolunteerPositionRole");
        }
    }
}
