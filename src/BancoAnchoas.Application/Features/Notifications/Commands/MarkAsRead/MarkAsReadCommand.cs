using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Notifications.Commands.MarkAsRead;

public record MarkAsReadCommand(int Id) : IRequest;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand>
{
    private readonly IRepository<Notification> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsReadCommandHandler(IRepository<Notification> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkAsReadCommand request, CancellationToken ct)
    {
        var notification = await _repository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Notification), request.Id);

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        _repository.Update(notification);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
