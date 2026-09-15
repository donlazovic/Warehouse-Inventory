using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Identity;

namespace Warehouse.DataLayer.Config.Identity;

internal class RolePermissionConfig : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermission", "wh");
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        builder.Property(x => x.GrantedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.Role)
               .WithMany(x => x.RolePermissions)
               .HasForeignKey(x => x.RoleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
               .WithMany(x => x.RolePermissions)
               .HasForeignKey(x => x.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
