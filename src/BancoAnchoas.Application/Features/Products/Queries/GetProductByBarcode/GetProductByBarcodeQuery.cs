using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Products.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Products.Queries.GetProductByBarcode;

public record GetProductByBarcodeQuery(string Barcode) : IRequest<ProductDto>;

public class GetProductByBarcodeQueryHandler : IRequestHandler<GetProductByBarcodeQuery, ProductDto>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    public GetProductByBarcodeQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByBarcodeQuery request, CancellationToken ct)
    {
        var product = await _repository.Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Barcode == request.Barcode, ct)
            ?? throw new NotFoundException(nameof(Product), request.Barcode);

        return _mapper.Map<ProductDto>(product);
    }
}
