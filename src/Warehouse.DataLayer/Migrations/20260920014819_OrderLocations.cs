using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class OrderLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationLocationId",
                schema: "wh",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceLocationId",
                schema: "wh",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_DestinationLocationId",
                schema: "wh",
                table: "Order",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SourceLocationId",
                schema: "wh",
                table: "Order",
                column: "SourceLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_StorageLocation_DestinationLocationId",
                schema: "wh",
                table: "Order",
                column: "DestinationLocationId",
                principalSchema: "wh",
                principalTable: "StorageLocation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_StorageLocation_SourceLocationId",
                schema: "wh",
                table: "Order",
                column: "SourceLocationId",
                principalSchema: "wh",
                principalTable: "StorageLocation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_StorageLocation_DestinationLocationId",
                schema: "wh",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_StorageLocation_SourceLocationId",
                schema: "wh",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_DestinationLocationId",
                schema: "wh",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_SourceLocationId",
                schema: "wh",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "DestinationLocationId",
                schema: "wh",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "SourceLocationId",
                schema: "wh",
                table: "Order");
        }
    }
}
