using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Entities.Notifications;

namespace Warehouse.DataLayer.Config.Notifications;

internal class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notification", "wh");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Link).HasMaxLength(200);

        builder.HasIndex(x => new { x.UserId, x.ReadAt, x.CreatedAt });

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
