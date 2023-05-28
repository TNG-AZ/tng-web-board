using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class addDmTrainingDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastDMTrainingDate",
                table: "Members",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDMTrainingDate",
                table: "Members");
        }
    }
}
