using BancoAnchoas.Application.Features.Stock.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Stock.Queries.GetWriteOffHistory;

public record GetWriteOffHistoryQuery(MovementReason? Reason) : IRequest<IReadOnlyList<StockMovementDto>>;

public class GetWriteOffHistoryQueryHandler : IRequestHandler<GetWriteOffHistoryQuery, IReadOnlyList<StockMovementDto>>
{
    private readonly IRepository<StockMovement> _repository;

    public GetWriteOffHistoryQueryHandler(IRepository<StockMovement> repository)
        => _repository = repository;

    public async Task<IReadOnlyList<StockMovementDto>> Handle(GetWriteOffHistoryQuery request, CancellationToken ct)
    {
        var query = _repository.Query()
            .Where(m => m.Type == MovementType.WriteOff)
            .Include(m => m.Product)
            .Include(m => m.Sector)
            .AsQueryable();

        if (request.Reason.HasValue)
            query = query.Where(m => m.Reason == request.Reason.Value);

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new StockMovementDto
            {
                Id = m.Id,
                Quantity = m.Quantity,
                Type = m.Type,
                Reason = m.Reason,
                Notes = m.Notes,
                ProductId = m.ProductId,
                ProductName = m.Product.Name,
                SectorId = m.SectorId,
                SectorName = m.Sector.Name,
                UserId = m.UserId,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);
    }
}
