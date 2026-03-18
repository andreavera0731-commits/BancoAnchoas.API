using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BancoAnchoas.Application.Features.Warehouses.Commands.UpdateWarehouse;

public record UpdateWarehouseCommand(int Id, string Name, string? Location) : IRequest;

public class UpdateWarehouseCommandValidator : FluentValidation.AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Location).MaximumLength(200).When(x => x.Location is not null);
    }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand>
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWarehouseCommandHandler(IRepository<Warehouse> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Warehouse), request.Id);

        warehouse.Name = request.Name;
        warehouse.Location = request.Location;
        warehouse.UpdatedAt = DateTime.UtcNow;

        _repository.Update(warehouse);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
