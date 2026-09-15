using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Catalog;

namespace Warehouse.DataLayer.Config.Catalog;

internal class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.ParentCategory)
               .WithMany(x => x.SubCategories)
               .HasForeignKey(x => x.ParentCategoryId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
