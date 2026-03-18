namespace BancoAnchoas.Application.Features.Warehouses.DTOs;

public class WarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
}
