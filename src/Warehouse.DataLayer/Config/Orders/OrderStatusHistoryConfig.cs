using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Orders;

namespace Warehouse.DataLayer.Config.Orders;

internal class OrderStatusHistoryConfig : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus).HasConversion<int?>();
        builder.Property(x => x.ToStatus).HasConversion<int>();
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => x.OrderId);

        builder.HasOne(x => x.Order)
               .WithMany(x => x.StatusHistory)
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedByUser)
               .WithMany()
               .HasForeignKey(x => x.ChangedByUserId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
