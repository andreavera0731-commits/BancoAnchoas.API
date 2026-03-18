using BancoAnchoas.API.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.API.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DatabaseInitializer));
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            if (db.Database.IsRelational())
                await db.Database.MigrateAsync();
            else
                await db.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al conectar/migrar la base de datos. Verifique la conexión.");
            throw;
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = ["Admin", "Almacenista"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var config = app.Configuration;
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var adminEmail = config["AdminSeed:Email"] ?? "admin@bancodeanchoas.com";

        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = config["AdminSeed:Name"] ?? "Administrador"
            };
            var result = await userManager.CreateAsync(admin, config["AdminSeed:Password"] ?? "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
