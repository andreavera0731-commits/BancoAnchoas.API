using BancoAnchoas.Domain.Common;
using BancoAnchoas.Domain.Enums;

namespace BancoAnchoas.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int Quantity { get; set; }
    public MovementType Type { get; set; }
    public AdjustmentType? AdjustmentType { get; set; }
    public MovementReason? Reason { get; set; }
    public string? Notes { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int SectorId { get; set; }
    public Sector Sector { get; set; } = null!;

    public int? FromSectorId { get; set; }
    public Sector? FromSector { get; set; }

    public string UserId { get; set; } = string.Empty;
}
