using BancoAnchoas.Application.Features.Warehouses.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Warehouses.Queries.GetSectorsByWarehouse;

public record GetSectorsByWarehouseQuery(int WarehouseId) : IRequest<IReadOnlyList<SectorDto>>;

public class GetSectorsByWarehouseQueryHandler : IRequestHandler<GetSectorsByWarehouseQuery, IReadOnlyList<SectorDto>>
{
    private readonly IRepository<Sector> _repository;

    public GetSectorsByWarehouseQueryHandler(IRepository<Sector> repository)
        => _repository = repository;

    public async Task<IReadOnlyList<SectorDto>> Handle(GetSectorsByWarehouseQuery request, CancellationToken ct)
    {
        return await _repository.Query()
            .Where(s => s.WarehouseId == request.WarehouseId)
            .Include(s => s.Warehouse)
            .Include(s => s.SectorCategories).ThenInclude(sc => sc.Category)
            .OrderBy(s => s.Name)
            .Select(s => new SectorDto
            {
                Id = s.Id,
                Name = s.Name,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse.Name,
                Categories = s.SectorCategories.Select(sc => new SectorCategoryDto
                {
                    Id = sc.Category.Id,
                    Name = sc.Category.Name
                }).ToList(),
                CreatedAt = s.CreatedAt
            })
            .ToListAsync(ct);
    }
}
