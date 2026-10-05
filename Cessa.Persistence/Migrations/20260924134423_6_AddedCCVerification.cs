using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cessa.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _6_AddedCCVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerifiedCreator",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerifiedCreator",
                table: "AspNetUsers");
        }
    }
}
