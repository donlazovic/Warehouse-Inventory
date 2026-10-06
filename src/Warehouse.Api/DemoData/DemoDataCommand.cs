using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Extensions;
using Warehouse.DataLayer.Context;

namespace Warehouse.Api.DemoData;

public static class DemoDataCommand
{
    public static async Task RunAsync(WebApplication app, bool seed)
    {
        if (!app.Environment.IsDevelopment())
        {
            Console.WriteLine("Komanda je dozvoljena samo u Development okruzenju.");
            return;
        }

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

            Console.WriteLine("Brisanje postojecih podataka (administrator ostaje)...");
            await DemoDataReset.RunAsync(db);
        }

        await app.SeedAdminUserAsync();

        if (!seed)
        {
            Console.WriteLine("Baza je ociscena.");
            return;
        }

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            db.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

            Console.WriteLine("Simulacija 90 dana rada lanca, ovo traje do minut...");
            var summary = await new DemoDataGenerator(db).RunAsync();
            Console.WriteLine(summary);
        }
    }
}

internal static class DemoDataReset
{
    private static readonly string[] ClearedTables =
    {
        "Notification", "PasswordResetToken", "StockMovement", "OrderStatusHistory", "OrderItem", "Order",
        "StockItem", "Product", "Category", "StorageLocation", "Store", "Supplier", "RefreshToken"
    };

    public static async Task RunAsync(WarehouseDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            DELETE FROM wh.Notification;
            DELETE FROM wh.PasswordResetToken;
            DELETE FROM wh.StockMovement;
            DELETE FROM wh.OrderStatusHistory;
            DELETE FROM wh.OrderItem;
            DELETE FROM wh.[Order];
            DELETE FROM wh.StockItem;
            DELETE FROM wh.UserFavoriteProduct;
            DELETE FROM wh.Product;
            UPDATE wh.Category SET ParentCategoryId = NULL;
            DELETE FROM wh.Category;
            DELETE FROM wh.StorageLocation;
            DELETE FROM wh.Store;
            DELETE FROM wh.Supplier;
            DELETE FROM wh.RefreshToken;
            UPDATE wh.[User] SET ApprovedByUserId = NULL;
            DELETE FROM wh.[User] WHERE IsOwner = 0 AND Email <> 'admin@warehouse.local';
            """);

        foreach (var table in ClearedTables)
        {
            await db.Database.ExecuteSqlRawAsync($"""
                IF EXISTS (SELECT 1 FROM sys.identity_columns
                           WHERE object_id = OBJECT_ID('wh.[{table}]') AND last_value IS NOT NULL)
                    DBCC CHECKIDENT ('wh.[{table}]', RESEED, 0);
                """);
        }

        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @max int = (SELECT ISNULL(MAX(Id), 0) FROM wh.[User]);
            DBCC CHECKIDENT ('wh.[User]', RESEED, @max);
            """);
    }
}
