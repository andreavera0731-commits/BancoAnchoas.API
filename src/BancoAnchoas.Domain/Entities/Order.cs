using BancoAnchoas.Domain.Common;
using BancoAnchoas.Domain.Enums;

namespace BancoAnchoas.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public string UserId { get; set; } = string.Empty;
}
