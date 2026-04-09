using BancoAnchoas.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.API.Infrastructure.Persistence;

public partial class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<SectorCategory> SectorCategories => Set<SectorCategory>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Requester> Requesters => Set<Requester>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
