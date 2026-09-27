using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class UserOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOwner",
                schema: "wh",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_User_IsOwner",
                schema: "wh",
                table: "User",
                column: "IsOwner",
                unique: true,
                filter: "[IsOwner] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_IsOwner",
                schema: "wh",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsOwner",
                schema: "wh",
                table: "User");
        }
    }
}
