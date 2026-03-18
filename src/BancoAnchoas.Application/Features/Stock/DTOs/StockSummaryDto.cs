namespace BancoAnchoas.Application.Features.Stock.DTOs;

public class StockSummaryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public string Unit { get; set; } = string.Empty;
}
