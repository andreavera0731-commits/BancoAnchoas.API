namespace BancoAnchoas.Application.Features.Warehouses.DTOs;

public class SectorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public IReadOnlyList<SectorCategoryDto> Categories { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class SectorCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
