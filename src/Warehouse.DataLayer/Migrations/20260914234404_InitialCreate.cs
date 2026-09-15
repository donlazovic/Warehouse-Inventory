using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Warehouse.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wh");

            migrationBuilder.CreateTable(
                name: "Category",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Category_Category_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalSchema: "wh",
                        principalTable: "Category",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permission_Permission_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "wh",
                        principalTable: "Permission",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Store",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Store", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UnitOfMeasure = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinStock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MaxStock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "wh",
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                schema: "wh",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "wh",
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "wh",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "wh",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocation",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLocation_Store_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "wh",
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Store_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "wh",
                        principalTable: "Store",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "wh",
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_User_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalSchema: "wh",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Order_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "wh",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "wh",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteProduct",
                schema: "wh",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteProduct", x => new { x.UserId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_UserFavoriteProduct_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "wh",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteProduct_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "wh",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockItem",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MinStockOverride = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxStockOverride = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StorageLocationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "wh",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockItem_StorageLocation_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalSchema: "wh",
                        principalTable: "StorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "wh",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "wh",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "wh",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_User_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalSchema: "wh",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockMovement",
                schema: "wh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FromLocationId = table.Column<int>(type: "int", nullable: true),
                    ToLocationId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovement_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "wh",
                        principalTable: "Order",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMovement_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "wh",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovement_StorageLocation_FromLocationId",
                        column: x => x.FromLocationId,
                        principalSchema: "wh",
                        principalTable: "StorageLocation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMovement_StorageLocation_ToLocationId",
                        column: x => x.ToLocationId,
                        principalSchema: "wh",
                        principalTable: "StorageLocation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockMovement_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "wh",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                schema: "wh",
                table: "Permission",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "Module", "Name", "ParentId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 0, "products", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Products", "Proizvodi", null, null },
                    { 6, 0, "categories", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Categories", "Kategorije", null, null },
                    { 11, 0, "stock", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stock", "Zalihe", null, null },
                    { 15, 0, "orders", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Nalozi", null, null },
                    { 23, 0, "suppliers", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Suppliers", "Dobavljaci", null, null },
                    { 28, 0, "stores", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stores", "Prodajni objekti", null, null },
                    { 33, 0, "reports", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reports", "Izvestaji", null, null },
                    { 36, 0, "users", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users", "Korisnici", null, null }
                });

            migrationBuilder.InsertData(
                schema: "wh",
                table: "Role",
                columns: new[] { "Id", "CreatedAt", "Description", "IsSystemRole", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pun pristup sistemu", true, "Admin", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Odobravanje naloga i izvestaji", true, "Menadzer", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Realizacija naloga i korekcija zaliha", true, "Magacioner", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Samo pregled", true, "Viewer", null }
                });

            migrationBuilder.InsertData(
                schema: "wh",
                table: "Permission",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "Module", "Name", "ParentId", "UpdatedAt" },
                values: new object[,]
                {
                    { 2, 1, "products.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Products", "Pregled", 1, null },
                    { 3, 2, "products.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Products", "Kreiranje", 1, null },
                    { 4, 3, "products.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Products", "Izmena", 1, null },
                    { 5, 4, "products.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Products", "Brisanje", 1, null },
                    { 7, 1, "categories.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Categories", "Pregled", 6, null },
                    { 8, 2, "categories.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Categories", "Kreiranje", 6, null },
                    { 9, 3, "categories.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Categories", "Izmena", 6, null },
                    { 10, 4, "categories.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Categories", "Brisanje", 6, null },
                    { 12, 1, "stock.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stock", "Pregled", 11, null },
                    { 13, 3, "stock.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stock", "Korekcija", 11, null },
                    { 14, 7, "stock.export", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stock", "Izvoz", 11, null },
                    { 16, 1, "orders.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Pregled", 15, null },
                    { 17, 2, "orders.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Kreiranje", 15, null },
                    { 18, 3, "orders.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Izmena", 15, null },
                    { 19, 4, "orders.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Brisanje", 15, null },
                    { 20, 5, "orders.approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Odobravanje", 15, null },
                    { 21, 6, "orders.execute", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Realizacija", 15, null },
                    { 22, 7, "orders.export", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Orders", "Izvoz", 15, null },
                    { 24, 1, "suppliers.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Suppliers", "Pregled", 23, null },
                    { 25, 2, "suppliers.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Suppliers", "Kreiranje", 23, null },
                    { 26, 3, "suppliers.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Suppliers", "Izmena", 23, null },
                    { 27, 4, "suppliers.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Suppliers", "Brisanje", 23, null },
                    { 29, 1, "stores.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stores", "Pregled", 28, null },
                    { 30, 2, "stores.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stores", "Kreiranje", 28, null },
                    { 31, 3, "stores.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stores", "Izmena", 28, null },
                    { 32, 4, "stores.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stores", "Brisanje", 28, null },
                    { 34, 1, "reports.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reports", "Pregled", 33, null },
                    { 35, 7, "reports.export", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reports", "Izvoz", 33, null },
                    { 37, 1, "users.view", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users", "Pregled", 36, null },
                    { 38, 2, "users.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users", "Kreiranje", 36, null },
                    { 39, 3, "users.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users", "Izmena", 36, null },
                    { 40, 4, "users.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users", "Brisanje", 36, null }
                });

            migrationBuilder.InsertData(
                schema: "wh",
                table: "RolePermission",
                columns: new[] { "PermissionId", "RoleId", "GrantedAt" },
                values: new object[,]
                {
                    { 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 37, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 38, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 39, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 40, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_Name",
                schema: "wh",
                table: "Category",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Category_ParentCategoryId",
                schema: "wh",
                table: "Category",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ApprovedByUserId",
                schema: "wh",
                table: "Order",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CreatedAt",
                schema: "wh",
                table: "Order",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CreatedByUserId",
                schema: "wh",
                table: "Order",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderNumber",
                schema: "wh",
                table: "Order",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_Status_OrderType",
                schema: "wh",
                table: "Order",
                columns: new[] { "Status", "OrderType" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_StoreId",
                schema: "wh",
                table: "Order",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SupplierId",
                schema: "wh",
                table: "Order",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                schema: "wh",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductId",
                schema: "wh",
                table: "OrderItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_ChangedByUserId",
                schema: "wh",
                table: "OrderStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OrderId",
                schema: "wh",
                table: "OrderStatusHistory",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Code",
                schema: "wh",
                table: "Permission",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_ParentId",
                schema: "wh",
                table: "Permission",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryId",
                schema: "wh",
                table: "Product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                schema: "wh",
                table: "Product",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Sku",
                schema: "wh",
                table: "Product",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_Token",
                schema: "wh",
                table: "RefreshToken",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserId",
                schema: "wh",
                table: "RefreshToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                schema: "wh",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                schema: "wh",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItem_ProductId_StorageLocationId",
                schema: "wh",
                table: "StockItem",
                columns: new[] { "ProductId", "StorageLocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItem_StorageLocationId",
                schema: "wh",
                table: "StockItem",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_CreatedAt",
                schema: "wh",
                table: "StockMovement",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_FromLocationId",
                schema: "wh",
                table: "StockMovement",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_OrderId",
                schema: "wh",
                table: "StockMovement",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_ProductId_CreatedAt",
                schema: "wh",
                table: "StockMovement",
                columns: new[] { "ProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_ToLocationId",
                schema: "wh",
                table: "StockMovement",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovement_UserId",
                schema: "wh",
                table: "StockMovement",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocation_Code",
                schema: "wh",
                table: "StorageLocation",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocation_StoreId",
                schema: "wh",
                table: "StorageLocation",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Store_Code",
                schema: "wh",
                table: "Store",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_Name",
                schema: "wh",
                table: "Supplier",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "wh",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                schema: "wh",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteProduct_ProductId",
                schema: "wh",
                table: "UserFavoriteProduct",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItem",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "RefreshToken",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "RolePermission",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "StockItem",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "StockMovement",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "UserFavoriteProduct",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "Permission",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "Order",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "StorageLocation",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "Supplier",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "User",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "Store",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "Category",
                schema: "wh");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "wh");
        }
    }
}
