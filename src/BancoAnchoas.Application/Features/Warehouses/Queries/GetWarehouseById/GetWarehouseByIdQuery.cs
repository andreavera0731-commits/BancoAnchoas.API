using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Warehouses.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace BancoAnchoas.Application.Features.Warehouses.Queries.GetWarehouseById;

public record GetWarehouseByIdQuery(int Id) : IRequest<WarehouseDto>;

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto>
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IMapper _mapper;

    public GetWarehouseByIdQueryHandler(IRepository<Warehouse> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<WarehouseDto> Handle(GetWarehouseByIdQuery request, CancellationToken ct)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Warehouse), request.Id);

        return _mapper.Map<WarehouseDto>(warehouse);
    }
}
