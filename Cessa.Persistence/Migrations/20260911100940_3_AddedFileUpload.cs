using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cessa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _3_AddedFileUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Posts");

            migrationBuilder.AddColumn<Guid>(
                name: "CoverFileId",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhotoFileId",
                table: "HallOfFameMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoverFileId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceFileId",
                table: "EventEnrollments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EventEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CoverFileId",
                table: "Posts",
                column: "CoverFileId");

            migrationBuilder.CreateIndex(
                name: "IX_HallOfFameMembers_PhotoFileId",
                table: "HallOfFameMembers",
                column: "PhotoFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CoverFileId",
                table: "Events",
                column: "CoverFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EventEnrollments_InvoiceFileId",
                table: "EventEnrollments",
                column: "InvoiceFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UploadedById",
                table: "UploadedFiles",
                column: "UploadedById");

            migrationBuilder.AddForeignKey(
                name: "FK_EventEnrollments_UploadedFiles_InvoiceFileId",
                table: "EventEnrollments",
                column: "InvoiceFileId",
                principalTable: "UploadedFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_UploadedFiles_CoverFileId",
                table: "Events",
                column: "CoverFileId",
                principalTable: "UploadedFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HallOfFameMembers_UploadedFiles_PhotoFileId",
                table: "HallOfFameMembers",
                column: "PhotoFileId",
                principalTable: "UploadedFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_UploadedFiles_CoverFileId",
                table: "Posts",
                column: "CoverFileId",
                principalTable: "UploadedFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventEnrollments_UploadedFiles_InvoiceFileId",
                table: "EventEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_UploadedFiles_CoverFileId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_HallOfFameMembers_UploadedFiles_PhotoFileId",
                table: "HallOfFameMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_UploadedFiles_CoverFileId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CoverFileId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_HallOfFameMembers_PhotoFileId",
                table: "HallOfFameMembers");

            migrationBuilder.DropIndex(
                name: "IX_Events_CoverFileId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventEnrollments_InvoiceFileId",
                table: "EventEnrollments");

            migrationBuilder.DropColumn(
                name: "CoverFileId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PhotoFileId",
                table: "HallOfFameMembers");

            migrationBuilder.DropColumn(
                name: "CoverFileId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "InvoiceFileId",
                table: "EventEnrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EventEnrollments");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
