using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Categories.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace BancoAnchoas.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IRepository<Category> _repository;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(IRepository<Category> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        return _mapper.Map<CategoryDto>(category);
    }
}
