using BancoAnchoas.Application.Features.Products.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Products.Queries.GetExpiringProducts;

public record GetExpiringProductsQuery : IRequest<IReadOnlyList<ProductListDto>>;

public class GetExpiringProductsQueryHandler : IRequestHandler<GetExpiringProductsQuery, IReadOnlyList<ProductListDto>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetExpiringProductsQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductListDto>> Handle(GetExpiringProductsQuery request, CancellationToken ct)
    {
        var limit = DateTime.UtcNow.AddDays(7);
        return await _repository.Query()
            .Where(p => p.ExpirationDate != null && p.ExpirationDate <= limit)
            .OrderBy(p => p.ExpirationDate)
            .ProjectToType<ProductListDto>(_mapper.Config)
            .ToListAsync(ct);
    }
}
