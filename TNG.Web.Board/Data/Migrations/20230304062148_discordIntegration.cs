using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class discordIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MembersDiscordIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscordId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembersDiscordIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembersDiscordIntegrations_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembersDiscordIntegrations_MemberId_DiscordId",
                table: "MembersDiscordIntegrations",
                columns: new[] { "MemberId", "DiscordId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembersDiscordIntegrations");
        }
    }
}
