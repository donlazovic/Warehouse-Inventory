using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Entities.Partners;
using Warehouse.DataLayer.Seed;

namespace Warehouse.DataLayer.Context;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<UserFavoriteProduct> UserFavoriteProducts => Set<UserFavoriteProduct>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Store> Stores => Set<Store>();

    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        PermissionSeed.Seed(modelBuilder);
        RoleSeed.Seed(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}
