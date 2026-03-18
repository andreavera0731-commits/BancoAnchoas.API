using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BancoAnchoas.API.Infrastructure.Services;

public class AlertCheckOptions
{
    public int IntervalHours { get; set; } = 2;
    public int ExpirationWarningDays { get; set; } = 7;
}

public class AlertNotificationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AlertCheckOptions _options;
    private readonly ILogger<AlertNotificationService> _logger;

    public AlertNotificationService(
        IServiceScopeFactory scopeFactory,
        IOptions<AlertCheckOptions> options,
        ILogger<AlertNotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAlerts(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking alerts");
            }

            await Task.Delay(TimeSpan.FromHours(_options.IntervalHours), stoppingToken);
        }
    }

    private async Task CheckAlerts(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var productRepo = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cutoff = DateTime.UtcNow.AddHours(-24);

        // Low stock
        var lowStock = await productRepo.Query()
            .Where(p => p.MinimumStock > 0 && p.Stock <= p.MinimumStock)
            .ToListAsync(ct);

        foreach (var product in lowStock)
        {
            if (await HasRecentNotification(notificationRepo, product.Id, NotificationType.LowStock, cutoff, ct))
                continue;

            await notificationRepo.AddAsync(new Notification
            {
                Title = $"Stock bajo: {product.Name}",
                Message = $"Stock actual: {product.Stock} {product.Unit}. Mínimo: {product.MinimumStock} {product.Unit}.",
                Type = NotificationType.LowStock,
                ProductId = product.Id
            }, ct);
        }

        // Expiring / Expired
        var expirationLimit = DateTime.UtcNow.AddDays(_options.ExpirationWarningDays);
        var expiring = await productRepo.Query()
            .Where(p => p.ExpirationDate != null && p.ExpirationDate <= expirationLimit && p.Stock > 0)
            .ToListAsync(ct);

        foreach (var product in expiring)
        {
            var isExpired = product.ExpirationDate < DateTime.UtcNow;
            var type = isExpired ? NotificationType.Expired : NotificationType.Expiring;

            if (await HasRecentNotification(notificationRepo, product.Id, type, cutoff, ct))
                continue;

            var title = isExpired
                ? $"Producto vencido: {product.Name}"
                : $"Próximo a vencer: {product.Name}";
            var message = $"Fecha de vencimiento: {product.ExpirationDate:yyyy-MM-dd}. Stock: {product.Stock} {product.Unit}.";

            await notificationRepo.AddAsync(new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                ProductId = product.Id
            }, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Alert check completed. Low stock: {LowStock}, Expiring: {Expiring}", lowStock.Count, expiring.Count);
    }

    private static async Task<bool> HasRecentNotification(
        IRepository<Notification> repo, int productId, NotificationType type, DateTime cutoff, CancellationToken ct)
    {
        return await repo.Query()
            .AnyAsync(n => n.ProductId == productId && n.Type == type && !n.IsRead && n.CreatedAt >= cutoff, ct);
    }
}
