using BancoAnchoas.Application.Features.Categories.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IRepository<Category> _repository;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(IRepository<Category> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        return await _repository.Query()
            .OrderBy(c => c.Name)
            .ProjectToType<CategoryDto>(_mapper.Config)
            .ToListAsync(ct);
    }
}
