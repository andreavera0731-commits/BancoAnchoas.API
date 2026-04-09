using BancoAnchoas.Domain.Common;

namespace BancoAnchoas.Domain.Entities;

public class Requester : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<StockMovement> StockMovements { get; set; } = [];
}
