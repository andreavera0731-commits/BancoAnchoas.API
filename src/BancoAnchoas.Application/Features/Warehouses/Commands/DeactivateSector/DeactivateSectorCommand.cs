using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Warehouses.Commands.DeactivateSector;

public record DeactivateSectorCommand(int Id) : IRequest;

public class DeactivateSectorCommandHandler : IRequestHandler<DeactivateSectorCommand>
{
    private readonly IRepository<Sector> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSectorCommandHandler(IRepository<Sector> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateSectorCommand request, CancellationToken ct)
    {
        var sector = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Sector), request.Id);

        sector.IsActive = false;
        sector.DeactivatedAt = DateTime.UtcNow;
        sector.UpdatedAt = DateTime.UtcNow;

        _repository.Update(sector);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
