using BancoAnchoas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoAnchoas.API.Infrastructure.Persistence.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(s => s.Warehouse)
               .WithMany(w => w.Sectors)
               .HasForeignKey(s => s.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
