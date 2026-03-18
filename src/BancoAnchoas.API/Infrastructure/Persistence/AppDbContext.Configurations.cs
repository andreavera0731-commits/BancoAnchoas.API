using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.API.Infrastructure.Persistence;

public partial class AppDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity tables

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global Query Filters — soft delete
        modelBuilder.Entity<Domain.Entities.Product>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Domain.Entities.Category>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Domain.Entities.Warehouse>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Domain.Entities.Sector>().HasQueryFilter(e => e.IsActive);
        // Matching filters for entities related to filtered parents
        modelBuilder.Entity<Domain.Entities.StockMovement>().HasQueryFilter(e => e.Product.IsActive);
        modelBuilder.Entity<Domain.Entities.SectorCategory>().HasQueryFilter(e => e.Category.IsActive && e.Sector.IsActive);
        modelBuilder.Entity<Domain.Entities.Notification>().HasQueryFilter(e => e.Product == null || e.Product.IsActive);

        SeedData(modelBuilder);
    }
}
