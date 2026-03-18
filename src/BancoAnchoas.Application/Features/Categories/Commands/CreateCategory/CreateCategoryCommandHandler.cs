using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(IRepository<Category> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        await _repository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return category.Id;
    }
}
