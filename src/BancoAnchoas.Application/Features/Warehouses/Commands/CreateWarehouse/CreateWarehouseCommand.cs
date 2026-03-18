using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BancoAnchoas.Application.Features.Warehouses.Commands.CreateWarehouse;

public record CreateWarehouseCommand(string Name, string? Location) : IRequest<int>;

public class CreateWarehouseCommandValidator : FluentValidation.AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Location).MaximumLength(200).When(x => x.Location is not null);
    }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, int>
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarehouseCommandHandler(IRepository<Warehouse> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = new Warehouse { Name = request.Name, Location = request.Location };
        await _repository.AddAsync(warehouse, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return warehouse.Id;
    }
}
