using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class UserApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "wh",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                schema: "wh",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_ApprovedByUserId",
                schema: "wh",
                table: "User",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_User_ApprovedByUserId",
                schema: "wh",
                table: "User",
                column: "ApprovedByUserId",
                principalSchema: "wh",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_User_ApprovedByUserId",
                schema: "wh",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_ApprovedByUserId",
                schema: "wh",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "wh",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "wh",
                table: "User");
        }
    }
}
