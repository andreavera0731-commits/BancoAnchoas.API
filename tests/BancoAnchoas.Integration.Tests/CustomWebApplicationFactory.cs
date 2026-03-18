using BancoAnchoas.API.Infrastructure.Persistence;
using BancoAnchoas.API.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BancoAnchoas.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Remove ALL existing DbContext registrations (runs AFTER app DI)
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<AppDbContext>();

            // Remove background service to avoid interference in tests
            services.RemoveAll<IHostedService>();

            // Register InMemory DB
            var dbName = "TestDb_" + Guid.NewGuid();
            services.AddSingleton<DbContextOptions<AppDbContext>>(_ =>
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(dbName)
                    .Options);
            services.AddScoped<AppDbContext>();
        });
    }
}
