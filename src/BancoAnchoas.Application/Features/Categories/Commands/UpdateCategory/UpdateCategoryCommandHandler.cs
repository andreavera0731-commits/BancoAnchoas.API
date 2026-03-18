using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(IRepository<Category> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        _repository.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
