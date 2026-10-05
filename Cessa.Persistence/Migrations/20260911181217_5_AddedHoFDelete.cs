using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cessa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _5_AddedHoFDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallOfFameMembers_UploadedFiles_PhotoFileId",
                table: "HallOfFameMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_HallOfFameMembers_UploadedFiles_PhotoFileId",
                table: "HallOfFameMembers",
                column: "PhotoFileId",
                principalTable: "UploadedFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallOfFameMembers_UploadedFiles_PhotoFileId",
                table: "HallOfFameMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_HallOfFameMembers_UploadedFiles_PhotoFileId",
                table: "HallOfFameMembers",
                column: "PhotoFileId",
                principalTable: "UploadedFiles",
                principalColumn: "Id");
        }
    }
}
