using BancoAnchoas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoAnchoas.API.Infrastructure.Persistence.Configurations;

public class SectorCategoryConfiguration : IEntityTypeConfiguration<SectorCategory>
{
    public void Configure(EntityTypeBuilder<SectorCategory> builder)
    {
        builder.HasKey(sc => new { sc.SectorId, sc.CategoryId });

        builder.HasOne(sc => sc.Sector)
               .WithMany(s => s.SectorCategories)
               .HasForeignKey(sc => sc.SectorId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sc => sc.Category)
               .WithMany(c => c.SectorCategories)
               .HasForeignKey(sc => sc.CategoryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
