using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Inventory;

namespace Warehouse.DataLayer.Config.Inventory;

internal class StorageLocationConfig : IEntityTypeConfiguration<StorageLocation>
{
    public void Configure(EntityTypeBuilder<StorageLocation> builder)
    {
        builder.ToTable("StorageLocation", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Zone).HasMaxLength(50);
        builder.Property(x => x.LocationType).HasConversion<int>();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.Store)
               .WithMany(x => x.StorageLocations)
               .HasForeignKey(x => x.StoreId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
