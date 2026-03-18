using BancoAnchoas.Application.Features.Warehouses.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Warehouses.Queries.GetWarehouses;

public record GetWarehousesQuery : IRequest<IReadOnlyList<WarehouseDto>>;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, IReadOnlyList<WarehouseDto>>
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IMapper _mapper;

    public GetWarehousesQueryHandler(IRepository<Warehouse> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken ct)
    {
        return await _repository.Query()
            .OrderBy(w => w.Name)
            .ProjectToType<WarehouseDto>(_mapper.Config)
            .ToListAsync(ct);
    }
}
