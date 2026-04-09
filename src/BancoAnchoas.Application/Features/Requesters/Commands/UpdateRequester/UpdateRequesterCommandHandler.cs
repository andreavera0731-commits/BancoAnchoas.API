using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Requesters.Commands.UpdateRequester;

public class UpdateRequesterCommandHandler : IRequestHandler<UpdateRequesterCommand>
{
    private readonly IRepository<Requester> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRequesterCommandHandler(IRepository<Requester> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateRequesterCommand request, CancellationToken ct)
    {
        var requester = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Requester), request.Id);

        requester.Name = request.Name;
        requester.Description = request.Description;
        requester.UpdatedAt = DateTime.UtcNow;

        _repository.Update(requester);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
