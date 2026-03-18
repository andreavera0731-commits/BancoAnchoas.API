using BancoAnchoas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoAnchoas.API.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.Product)
               .WithMany(p => p.StockMovements)
               .HasForeignKey(m => m.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Sector)
               .WithMany(s => s.StockMovements)
               .HasForeignKey(m => m.SectorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.FromSector)
               .WithMany(s => s.IncomingRelocations)
               .HasForeignKey(m => m.FromSectorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.UserId).IsRequired();
    }
}
