using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Requesters.Commands.DeactivateRequester;

public record DeactivateRequesterCommand(int Id) : IRequest;

public class DeactivateRequesterCommandHandler : IRequestHandler<DeactivateRequesterCommand>
{
    private readonly IRepository<Requester> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateRequesterCommandHandler(IRepository<Requester> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateRequesterCommand request, CancellationToken ct)
    {
        var requester = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Requester), request.Id);

        requester.IsActive = false;
        requester.DeactivatedAt = DateTime.UtcNow;
        requester.UpdatedAt = DateTime.UtcNow;

        _repository.Update(requester);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
