using BancoAnchoas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoAnchoas.API.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(500);

        builder.HasOne(n => n.Product)
               .WithMany(p => p.Notifications)
               .HasForeignKey(n => n.ProductId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
