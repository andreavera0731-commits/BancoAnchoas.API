using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Products.Commands.DeactivateProduct;

public class DeactivateProductCommandHandler : IRequestHandler<DeactivateProductCommand>
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateProductCommand request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.IsActive = false;
        product.DeactivatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _repository.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
