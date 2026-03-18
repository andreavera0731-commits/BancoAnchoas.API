using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Warehouses.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Warehouses.Queries.GetSectorById;

public record GetSectorByIdQuery(int Id) : IRequest<SectorDto>;

public class GetSectorByIdQueryHandler : IRequestHandler<GetSectorByIdQuery, SectorDto>
{
    private readonly IRepository<Sector> _repository;

    public GetSectorByIdQueryHandler(IRepository<Sector> repository)
        => _repository = repository;

    public async Task<SectorDto> Handle(GetSectorByIdQuery request, CancellationToken ct)
    {
        var sector = await _repository.Query()
            .Where(s => s.Id == request.Id)
            .Include(s => s.Warehouse)
            .Include(s => s.SectorCategories).ThenInclude(sc => sc.Category)
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
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Sector), request.Id);

        return sector;
    }
}
