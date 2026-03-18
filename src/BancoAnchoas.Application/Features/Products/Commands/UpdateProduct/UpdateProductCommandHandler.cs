using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Barcode = request.Barcode;
        product.Price = request.Price;
        product.Unit = request.Unit;
        product.MinimumStock = request.MinimumStock;
        product.ExpirationDate = request.ExpirationDate;
        product.Supplier = request.Supplier;
        product.CategoryId = request.CategoryId;
        product.DefaultSectorId = request.DefaultSectorId;
        product.UpdatedAt = DateTime.UtcNow;

        _repository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
