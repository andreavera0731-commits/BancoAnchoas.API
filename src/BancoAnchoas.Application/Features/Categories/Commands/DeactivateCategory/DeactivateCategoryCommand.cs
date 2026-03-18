using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Categories.Commands.DeactivateCategory;

public record DeactivateCategoryCommand(int Id) : IRequest;

public class DeactivateCategoryCommandHandler : IRequestHandler<DeactivateCategoryCommand>
{
    private readonly IRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCategoryCommandHandler(IRepository<Category> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateCategoryCommand request, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        category.IsActive = false;
        category.DeactivatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _repository.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
