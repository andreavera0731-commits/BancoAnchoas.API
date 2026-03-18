using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Notifications.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand : IRequest;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand>
{
    private readonly IRepository<Notification> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllAsReadCommandHandler(IRepository<Notification> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkAllAsReadCommand request, CancellationToken ct)
    {
        var unread = await _repository.Query()
            .Where(n => !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            _repository.Update(n);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
