using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNG.Web.Board.Data.Migrations
{
    /// <inheritdoc />
    public partial class noteTagsMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteTag_MemberNotes_NoteId",
                table: "NoteTag");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteTag_Tag_TagId",
                table: "NoteTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NoteTag",
                table: "NoteTag");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "NoteTag",
                newName: "NoteTagMappings");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_Name",
                table: "Tags",
                newName: "IX_Tags_Name");

            migrationBuilder.RenameIndex(
                name: "IX_NoteTag_TagId",
                table: "NoteTagMappings",
                newName: "IX_NoteTagMappings_TagId");

            migrationBuilder.RenameIndex(
                name: "IX_NoteTag_NoteId",
                table: "NoteTagMappings",
                newName: "IX_NoteTagMappings_NoteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoteTagMappings",
                table: "NoteTagMappings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteTagMappings_MemberNotes_NoteId",
                table: "NoteTagMappings",
                column: "NoteId",
                principalTable: "MemberNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteTagMappings_Tags_TagId",
                table: "NoteTagMappings",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteTagMappings_MemberNotes_NoteId",
                table: "NoteTagMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteTagMappings_Tags_TagId",
                table: "NoteTagMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NoteTagMappings",
                table: "NoteTagMappings");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.RenameTable(
                name: "NoteTagMappings",
                newName: "NoteTag");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_Name",
                table: "Tag",
                newName: "IX_Tag_Name");

            migrationBuilder.RenameIndex(
                name: "IX_NoteTagMappings_TagId",
                table: "NoteTag",
                newName: "IX_NoteTag_TagId");

            migrationBuilder.RenameIndex(
                name: "IX_NoteTagMappings_NoteId",
                table: "NoteTag",
                newName: "IX_NoteTag_NoteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoteTag",
                table: "NoteTag",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteTag_MemberNotes_NoteId",
                table: "NoteTag",
                column: "NoteId",
                principalTable: "MemberNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteTag_Tag_TagId",
                table: "NoteTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
