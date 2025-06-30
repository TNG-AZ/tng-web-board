using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class Volunteering1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerEventSlot_VolunteerPosition_PositionId",
                table: "VolunteerEventSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPosition_VolunteerPositionRole_RequiredRoleId",
                table: "VolunteerPosition");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositionRole_Members_MemberId",
                table: "VolunteerPositionRole");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositionRole_VolunteerPositionRole_VolunteerPositionRoleId",
                table: "VolunteerPositionRole");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerSlotMember_Members_MemberId",
                table: "VolunteerSlotMember");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerSlotMember_VolunteerEventSlot_SlotId",
                table: "VolunteerSlotMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerSlotMember",
                table: "VolunteerSlotMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerPositionRole",
                table: "VolunteerPositionRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerPosition",
                table: "VolunteerPosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerEventSlot",
                table: "VolunteerEventSlot");

            migrationBuilder.RenameTable(
                name: "VolunteerSlotMember",
                newName: "VolunteerSlotMembers");

            migrationBuilder.RenameTable(
                name: "VolunteerPositionRole",
                newName: "VolunteerPositionRoles");

            migrationBuilder.RenameTable(
                name: "VolunteerPosition",
                newName: "VolunteerPositions");

            migrationBuilder.RenameTable(
                name: "VolunteerEventSlot",
                newName: "VolunteerEventSlots");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerSlotMember_SlotId",
                table: "VolunteerSlotMembers",
                newName: "IX_VolunteerSlotMembers_SlotId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerSlotMember_MemberId",
                table: "VolunteerSlotMembers",
                newName: "IX_VolunteerSlotMembers_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerPositionRole_VolunteerPositionRoleId",
                table: "VolunteerPositionRoles",
                newName: "IX_VolunteerPositionRoles_VolunteerPositionRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerPositionRole_MemberId",
                table: "VolunteerPositionRoles",
                newName: "IX_VolunteerPositionRoles_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerPosition_RequiredRoleId",
                table: "VolunteerPositions",
                newName: "IX_VolunteerPositions_RequiredRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerEventSlot_PositionId",
                table: "VolunteerEventSlots",
                newName: "IX_VolunteerEventSlots_PositionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerSlotMembers",
                table: "VolunteerSlotMembers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerPositionRoles",
                table: "VolunteerPositionRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerPositions",
                table: "VolunteerPositions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerEventSlots",
                table: "VolunteerEventSlots",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "VolunteerRoleMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approval = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerRoleMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VolunteerRoleMembers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VolunteerRoleMembers_VolunteerPositionRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "VolunteerPositionRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerRoleMembers_MemberId",
                table: "VolunteerRoleMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerRoleMembers_RoleId",
                table: "VolunteerRoleMembers",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerEventSlots_VolunteerPositions_PositionId",
                table: "VolunteerEventSlots",
                column: "PositionId",
                principalTable: "VolunteerPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositionRoles_Members_MemberId",
                table: "VolunteerPositionRoles",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositionRoles_VolunteerPositionRoles_VolunteerPositionRoleId",
                table: "VolunteerPositionRoles",
                column: "VolunteerPositionRoleId",
                principalTable: "VolunteerPositionRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositions_VolunteerPositionRoles_RequiredRoleId",
                table: "VolunteerPositions",
                column: "RequiredRoleId",
                principalTable: "VolunteerPositionRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerSlotMembers_Members_MemberId",
                table: "VolunteerSlotMembers",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerSlotMembers_VolunteerEventSlots_SlotId",
                table: "VolunteerSlotMembers",
                column: "SlotId",
                principalTable: "VolunteerEventSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerEventSlots_VolunteerPositions_PositionId",
                table: "VolunteerEventSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositionRoles_Members_MemberId",
                table: "VolunteerPositionRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositionRoles_VolunteerPositionRoles_VolunteerPositionRoleId",
                table: "VolunteerPositionRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerPositions_VolunteerPositionRoles_RequiredRoleId",
                table: "VolunteerPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerSlotMembers_Members_MemberId",
                table: "VolunteerSlotMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerSlotMembers_VolunteerEventSlots_SlotId",
                table: "VolunteerSlotMembers");

            migrationBuilder.DropTable(
                name: "VolunteerRoleMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerSlotMembers",
                table: "VolunteerSlotMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerPositions",
                table: "VolunteerPositions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerPositionRoles",
                table: "VolunteerPositionRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerEventSlots",
                table: "VolunteerEventSlots");

            migrationBuilder.RenameTable(
                name: "VolunteerSlotMembers",
                newName: "VolunteerSlotMember");

            migrationBuilder.RenameTable(
                name: "VolunteerPositions",
                newName: "VolunteerPosition");

            migrationBuilder.RenameTable(
                name: "VolunteerPositionRoles",
                newName: "VolunteerPositionRole");

            migrationBuilder.RenameTable(
                name: "VolunteerEventSlots",
                newName: "VolunteerEventSlot");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerSlotMembers_SlotId",
                table: "VolunteerSlotMember",
                newName: "IX_VolunteerSlotMember_SlotId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerSlotMembers_MemberId",
                table: "VolunteerSlotMember",
                newName: "IX_VolunteerSlotMember_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerPositions_RequiredRoleId",
                table: "VolunteerPosition",
                newName: "IX_VolunteerPosition_RequiredRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerPositionRoles_VolunteerPositionRoleId",
                table: "VolunteerPositionRole",
                newName: "IX_VolunteerPositionRole_VolunteerPositionRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerPositionRoles_MemberId",
                table: "VolunteerPositionRole",
                newName: "IX_VolunteerPositionRole_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_VolunteerEventSlots_PositionId",
                table: "VolunteerEventSlot",
                newName: "IX_VolunteerEventSlot_PositionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerSlotMember",
                table: "VolunteerSlotMember",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerPosition",
                table: "VolunteerPosition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerPositionRole",
                table: "VolunteerPositionRole",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerEventSlot",
                table: "VolunteerEventSlot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerEventSlot_VolunteerPosition_PositionId",
                table: "VolunteerEventSlot",
                column: "PositionId",
                principalTable: "VolunteerPosition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPosition_VolunteerPositionRole_RequiredRoleId",
                table: "VolunteerPosition",
                column: "RequiredRoleId",
                principalTable: "VolunteerPositionRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositionRole_Members_MemberId",
                table: "VolunteerPositionRole",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerPositionRole_VolunteerPositionRole_VolunteerPositionRoleId",
                table: "VolunteerPositionRole",
                column: "VolunteerPositionRoleId",
                principalTable: "VolunteerPositionRole",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerSlotMember_Members_MemberId",
                table: "VolunteerSlotMember",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerSlotMember_VolunteerEventSlot_SlotId",
                table: "VolunteerSlotMember",
                column: "SlotId",
                principalTable: "VolunteerEventSlot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
