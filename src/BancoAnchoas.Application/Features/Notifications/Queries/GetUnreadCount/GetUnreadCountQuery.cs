using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery : IRequest<int>;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly IRepository<Notification> _repository;

    public GetUnreadCountQueryHandler(IRepository<Notification> repository)
        => _repository = repository;

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken ct)
    {
        return await _repository.Query().CountAsync(n => !n.IsRead, ct);
    }
}
