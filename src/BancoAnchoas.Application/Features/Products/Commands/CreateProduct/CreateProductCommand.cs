using MediatR;

namespace BancoAnchoas.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    string? Barcode,
    decimal? Price,
    string Unit,
    int Stock,
    int MinimumStock,
    DateTime? ExpirationDate,
    string? Supplier,
    int CategoryId,
    int? DefaultSectorId) : IRequest<int>;
