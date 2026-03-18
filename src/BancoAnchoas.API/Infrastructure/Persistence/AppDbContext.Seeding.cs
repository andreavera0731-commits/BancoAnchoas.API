using BancoAnchoas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.API.Infrastructure.Persistence;

public partial class AppDbContext
{
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse { Id = 1, Name = "Almacén Central", Location = "Sede Principal", CreatedAt = seedDate }
        );

        modelBuilder.Entity<Sector>().HasData(
            new Sector { Id = 1, Name = "Panadería", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 2, Name = "Cocina", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 3, Name = "Chocolatería", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 4, Name = "CAVA", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 5, Name = "Bacha", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 6, Name = "Huerta", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 7, Name = "Bioquímica", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 8, Name = "Sostenibilidad", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 9, Name = "Administración", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 10, Name = "Mantenimiento", WarehouseId = 1, CreatedAt = seedDate },
            new Sector { Id = 11, Name = "Depósito", WarehouseId = 1, CreatedAt = seedDate }
        );
    }
}
