using BancoAnchoas.Domain.Enums;

namespace BancoAnchoas.Application.Features.Stock.DTOs;

public class StockMovementDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public MovementType Type { get; set; }
    public AdjustmentType? AdjustmentType { get; set; }
    public MovementReason? Reason { get; set; }
    public string? Notes { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public int? FromSectorId { get; set; }
    public string? FromSectorName { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
