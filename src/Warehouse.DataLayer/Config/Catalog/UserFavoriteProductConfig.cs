using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Catalog;

namespace Warehouse.DataLayer.Config.Catalog;

internal class UserFavoriteProductConfig : IEntityTypeConfiguration<UserFavoriteProduct>
{
    public void Configure(EntityTypeBuilder<UserFavoriteProduct> builder)
    {
        builder.ToTable("UserFavoriteProduct", "wh");
        builder.HasKey(x => new { x.UserId, x.ProductId });

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.User)
               .WithMany(x => x.FavoriteProducts)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
               .WithMany(x => x.FavoritedBy)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
