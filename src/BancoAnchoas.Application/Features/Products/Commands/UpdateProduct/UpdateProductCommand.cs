using MediatR;

namespace BancoAnchoas.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    string? Barcode,
    decimal? Price,
    string Unit,
    int MinimumStock,
    DateTime? ExpirationDate,
    string? Supplier,
    int CategoryId,
    int? DefaultSectorId) : IRequest;
