using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Products.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;

namespace BancoAnchoas.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(string? Search, int? CategoryId, int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<ProductListDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductListDto>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ProductListDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.Sku.Contains(request.Search));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        query = query.OrderByDescending(p => p.CreatedAt);

        return await PaginatedList<ProductListDto>.CreateAsync(
            query.ProjectToType<ProductListDto>(_mapper.Config),
            request.PageNumber,
            request.PageSize,
            ct);
    }
}
