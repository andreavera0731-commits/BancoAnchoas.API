using BancoAnchoas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BancoAnchoas.API.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.Barcode).HasMaxLength(100);
        builder.HasIndex(p => p.Barcode).IsUnique().HasFilter("\"Barcode\" IS NOT NULL");
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Unit).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Supplier).HasMaxLength(200);

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DefaultSector)
               .WithMany(s => s.DefaultProducts)
               .HasForeignKey(p => p.DefaultSectorId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
