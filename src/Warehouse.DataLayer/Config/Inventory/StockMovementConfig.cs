using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Inventory;

namespace Warehouse.DataLayer.Config.Inventory;

internal class StockMovementConfig : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovement", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MovementType).HasConversion<int>();
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.ProductId, x.CreatedAt });

        builder.HasOne(x => x.Product)
               .WithMany(x => x.StockMovements)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromLocation)
               .WithMany()
               .HasForeignKey(x => x.FromLocationId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ToLocation)
               .WithMany()
               .HasForeignKey(x => x.ToLocationId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Order)
               .WithMany(x => x.StockMovements)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
