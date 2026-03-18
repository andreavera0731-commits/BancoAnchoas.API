using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Products.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _repository.Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        return _mapper.Map<ProductDto>(product);
    }
}
