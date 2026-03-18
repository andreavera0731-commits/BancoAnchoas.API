using BancoAnchoas.Domain.Common;

namespace BancoAnchoas.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal? Price { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Supplier { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? DefaultSectorId { get; set; }
    public Sector? DefaultSector { get; set; }

    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
