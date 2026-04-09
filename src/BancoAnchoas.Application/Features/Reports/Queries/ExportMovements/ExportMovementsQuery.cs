using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Stock.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Reports.Queries.ExportMovements;

public record ExportMovementsQuery(
    int? ProductId,
    int? SectorId,
    MovementType? Type,
    int? RequesterId,
    DateTime? From,
    DateTime? To,
    ReportFormat Format = ReportFormat.Csv) : IRequest<ReportFile>;

public class ExportMovementsQueryHandler : IRequestHandler<ExportMovementsQuery, ReportFile>
{
    private const int MaxRows = 100_000;

    private readonly IRepository<StockMovement> _repository;
    private readonly IReportGeneratorFactory _factory;

    public ExportMovementsQueryHandler(IRepository<StockMovement> repository, IReportGeneratorFactory factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async Task<ReportFile> Handle(ExportMovementsQuery request, CancellationToken ct)
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
        if (request.RequesterId.HasValue)
            query = query.Where(m => m.RequesterId == request.RequesterId.Value);
        if (request.From.HasValue)
            query = query.Where(m => m.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(m => m.CreatedAt <= request.To.Value);

        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(MaxRows)
            .Select(m => new StockMovementDto
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
            })
            .ToListAsync(ct);

        var generator = _factory.Create(request.Format);
        var content = generator.Generate(movements);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        return new ReportFile
        {
            Content = content,
            ContentType = generator.ContentType,
            FileName = $"movimientos-{timestamp}.{generator.FileExtension}"
        };
    }
}
