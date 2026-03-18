using BancoAnchoas.Domain.Common;
using BancoAnchoas.Domain.Enums;

namespace BancoAnchoas.Domain.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
