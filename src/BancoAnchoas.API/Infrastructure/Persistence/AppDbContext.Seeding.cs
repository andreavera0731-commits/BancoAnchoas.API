using BancoAnchoas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.API.Infrastructure.Persistence;

public partial class AppDbContext
{
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed data removed — warehouses and sectors are now managed via the API.
    }
}
