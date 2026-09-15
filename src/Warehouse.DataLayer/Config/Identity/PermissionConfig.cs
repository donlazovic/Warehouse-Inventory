using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Identity;

namespace Warehouse.DataLayer.Config.Identity;

internal class PermissionConfig : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permission", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Action).HasConversion<int>();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne(x => x.Parent)
               .WithMany(x => x.Children)
               .HasForeignKey(x => x.ParentId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
