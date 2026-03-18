using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BancoAnchoas.Application.Features.Warehouses.Commands.CreateSector;

public record CreateSectorCommand(string Name, int WarehouseId) : IRequest<int>;

public class CreateSectorCommandValidator : FluentValidation.AbstractValidator<CreateSectorCommand>
{
    public CreateSectorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
    }
}

public class CreateSectorCommandHandler : IRequestHandler<CreateSectorCommand, int>
{
    private readonly IRepository<Sector> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSectorCommandHandler(IRepository<Sector> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateSectorCommand request, CancellationToken ct)
    {
        var sector = new Sector { Name = request.Name, WarehouseId = request.WarehouseId };
        await _repository.AddAsync(sector, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return sector.Id;
    }
}
