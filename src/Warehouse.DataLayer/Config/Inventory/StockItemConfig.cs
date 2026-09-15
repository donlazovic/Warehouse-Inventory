using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Inventory;

namespace Warehouse.DataLayer.Config.Inventory;

internal class StockItemConfig : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItem", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.MinStockOverride).HasPrecision(18, 3);
        builder.Property(x => x.MaxStockOverride).HasPrecision(18, 3);

        builder.Ignore(x => x.EffectiveMinStock);
        builder.Ignore(x => x.EffectiveMaxStock);
        builder.Ignore(x => x.IsBelowMinimum);

        builder.HasIndex(x => new { x.ProductId, x.StorageLocationId }).IsUnique();

        builder.HasOne(x => x.Product)
               .WithMany(x => x.StockItems)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StorageLocation)
               .WithMany(x => x.StockItems)
               .HasForeignKey(x => x.StorageLocationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
