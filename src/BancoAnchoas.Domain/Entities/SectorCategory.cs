namespace BancoAnchoas.Domain.Entities;

public class SectorCategory
{
    public int SectorId { get; set; }
    public Sector Sector { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
