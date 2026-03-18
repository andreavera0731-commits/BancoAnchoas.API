using BancoAnchoas.Domain.Common;

namespace BancoAnchoas.Domain.Entities;

public class Sector : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public ICollection<SectorCategory> SectorCategories { get; set; } = [];
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public ICollection<StockMovement> IncomingRelocations { get; set; } = [];
    public ICollection<Product> DefaultProducts { get; set; } = [];
}
