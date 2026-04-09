using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Stock.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Stock.Queries.GetMovementHistory;

public record GetMovementHistoryQuery(
    int? ProductId,
    int? SectorId,
    MovementType? Type,
    DateTime? From,
    DateTime? To,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<StockMovementDto>>;

public class GetMovementHistoryQueryHandler : IRequestHandler<GetMovementHistoryQuery, PaginatedList<StockMovementDto>>
{
    private readonly IRepository<StockMovement> _repository;

    public GetMovementHistoryQueryHandler(IRepository<StockMovement> repository)
        => _repository = repository;

    public async Task<PaginatedList<StockMovementDto>> Handle(GetMovementHistoryQuery request, CancellationToken ct)
    {
        var query = _repository.Query()
            .Include(m => m.Product)
            .Include(m => m.Sector)
            .Include(m => m.FromSector)
            .Include(m => m.Requester)
            .AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(m => m.ProductId == request.ProductId.Value);
        if (request.SectorId.HasValue)
            query = query.Where(m => m.SectorId == request.SectorId.Value);
        if (request.Type.HasValue)
            query = query.Where(m => m.Type == request.Type.Value);
        if (request.From.HasValue)
            query = query.Where(m => m.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(m => m.CreatedAt <= request.To.Value);

        query = query.OrderByDescending(m => m.CreatedAt);

        var projected = query.Select(m => new StockMovementDto
        {
            Id = m.Id,
            Quantity = m.Quantity,
            Type = m.Type,
            AdjustmentType = m.AdjustmentType,
            Reason = m.Reason,
            Notes = m.Notes,
            ProductId = m.ProductId,
            ProductName = m.Product.Name,
            SectorId = m.SectorId,
            SectorName = m.Sector.Name,
            FromSectorId = m.FromSectorId,
            FromSectorName = m.FromSector != null ? m.FromSector.Name : null,
            RequesterId = m.RequesterId,
            RequesterName = m.Requester != null ? m.Requester.Name : null,
            UserId = m.UserId,
            CreatedAt = m.CreatedAt
        });

        return await PaginatedList<StockMovementDto>.CreateAsync(projected, request.PageNumber, request.PageSize, ct);
    }
}
