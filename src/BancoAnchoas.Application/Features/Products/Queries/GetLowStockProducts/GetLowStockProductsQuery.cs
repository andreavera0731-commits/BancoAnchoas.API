using BancoAnchoas.Application.Features.Products.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Products.Queries.GetLowStockProducts;

public record GetLowStockProductsQuery : IRequest<IReadOnlyList<ProductListDto>>;

public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, IReadOnlyList<ProductListDto>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetLowStockProductsQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductListDto>> Handle(GetLowStockProductsQuery request, CancellationToken ct)
    {
        return await _repository.Query()
            .Where(p => p.MinimumStock > 0 && p.Stock <= p.MinimumStock)
            .OrderBy(p => p.Stock)
            .ProjectToType<ProductListDto>(_mapper.Config)
            .ToListAsync(ct);
    }
}
