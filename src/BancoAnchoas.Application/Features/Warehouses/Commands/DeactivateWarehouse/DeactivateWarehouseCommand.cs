using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Warehouses.Commands.DeactivateWarehouse;

public record DeactivateWarehouseCommand(int Id) : IRequest;

public class DeactivateWarehouseCommandHandler : IRequestHandler<DeactivateWarehouseCommand>
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IRepository<Sector> _sectorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateWarehouseCommandHandler(
        IRepository<Warehouse> repository,
        IRepository<Sector> sectorRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _sectorRepository = sectorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Warehouse), request.Id);

        var hasActiveSectors = await _sectorRepository.Query()
            .AnyAsync(s => s.WarehouseId == request.Id, ct);

        if (hasActiveSectors)
            throw new Common.Exceptions.ValidationException(
                new[] { new FluentValidation.Results.ValidationFailure("Id", "Cannot deactivate a warehouse with active sectors.") });

        warehouse.IsActive = false;
        warehouse.DeactivatedAt = DateTime.UtcNow;
        warehouse.UpdatedAt = DateTime.UtcNow;

        _repository.Update(warehouse);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
