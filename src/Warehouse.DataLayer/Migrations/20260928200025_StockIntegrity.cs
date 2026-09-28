using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Warehouse.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class StockIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IssueReason",
                schema: "wh",
                table: "StockMovement",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "wh",
                table: "StockItem",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "wh",
                table: "Order",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.InsertData(
                schema: "wh",
                table: "Permission",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "Module", "Name", "ParentId", "UpdatedAt" },
                values: new object[] { 41, 6, "stock.issue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stock", "Izlaz robe", 11, null });

            migrationBuilder.InsertData(
                schema: "wh",
                table: "RolePermission",
                columns: new[] { "PermissionId", "RoleId", "GrantedAt" },
                values: new object[,]
                {
                    { 41, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockMovement_HasLocation",
                schema: "wh",
                table: "StockMovement",
                sql: "[FromLocationId] IS NOT NULL OR [ToLocationId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockMovement_Quantity_Positive",
                schema: "wh",
                table: "StockMovement",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StockItem_Quantity_NonNegative",
                schema: "wh",
                table: "StockItem",
                sql: "[Quantity] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StockMovement_HasLocation",
                schema: "wh",
                table: "StockMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockMovement_Quantity_Positive",
                schema: "wh",
                table: "StockMovement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StockItem_Quantity_NonNegative",
                schema: "wh",
                table: "StockItem");

            migrationBuilder.DeleteData(
                schema: "wh",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 41, 1 });

            migrationBuilder.DeleteData(
                schema: "wh",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 41, 2 });

            migrationBuilder.DeleteData(
                schema: "wh",
                table: "RolePermission",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 41, 3 });

            migrationBuilder.DeleteData(
                schema: "wh",
                table: "Permission",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DropColumn(
                name: "IssueReason",
                schema: "wh",
                table: "StockMovement");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "wh",
                table: "StockItem");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "wh",
                table: "Order");
        }
    }
}
