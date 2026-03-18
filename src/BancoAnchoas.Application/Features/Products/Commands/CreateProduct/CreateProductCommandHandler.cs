using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Barcode = request.Barcode,
            Price = request.Price,
            Unit = request.Unit,
            Stock = request.Stock,
            MinimumStock = request.MinimumStock,
            ExpirationDate = request.ExpirationDate,
            Supplier = request.Supplier,
            CategoryId = request.CategoryId,
            DefaultSectorId = request.DefaultSectorId
        };

        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Auto-generate SKU after Id is assigned
        product.Sku = $"PROD-{product.Id:D5}";
        _repository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return product.Id;
    }
}
