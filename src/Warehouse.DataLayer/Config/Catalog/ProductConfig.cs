using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Catalog;

namespace Warehouse.DataLayer.Config.Catalog;

internal class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.UnitOfMeasure).HasConversion<int>();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.MinStock).HasPrecision(18, 3);
        builder.Property(x => x.MaxStock).HasPrecision(18, 3);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.Category)
               .WithMany(x => x.Products)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
