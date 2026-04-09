using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Requesters.Commands.CreateRequester;

public class CreateRequesterCommandHandler : IRequestHandler<CreateRequesterCommand, int>
{
    private readonly IRepository<Requester> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRequesterCommandHandler(IRepository<Requester> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateRequesterCommand request, CancellationToken ct)
    {
        var requester = new Requester
        {
            Name = request.Name,
            Description = request.Description
        };

        await _repository.AddAsync(requester, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return requester.Id;
    }
}
