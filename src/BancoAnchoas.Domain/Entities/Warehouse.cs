using BancoAnchoas.Domain.Common;

namespace BancoAnchoas.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }

    public ICollection<Sector> Sectors { get; set; } = [];
}
