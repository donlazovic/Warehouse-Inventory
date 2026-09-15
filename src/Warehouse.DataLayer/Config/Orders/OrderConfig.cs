using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Orders;

namespace Warehouse.DataLayer.Config.Orders;

internal class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Order", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OrderType).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.TotalValue).HasPrecision(18, 2);
        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => new { x.Status, x.OrderType });
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Supplier)
               .WithMany(x => x.Orders)
               .HasForeignKey(x => x.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Store)
               .WithMany(x => x.Orders)
               .HasForeignKey(x => x.StoreId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
               .WithMany()
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ApprovedByUser)
               .WithMany()
               .HasForeignKey(x => x.ApprovedByUserId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
