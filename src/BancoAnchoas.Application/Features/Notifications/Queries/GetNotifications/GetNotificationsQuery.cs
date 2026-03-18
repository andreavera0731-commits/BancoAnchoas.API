using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Notifications.DTOs;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BancoAnchoas.Application.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(bool? IsRead, int PageNumber = 1, int PageSize = 20)
    : IRequest<PaginatedList<NotificationDto>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IRepository<Notification> _repository;
    private readonly IMapper _mapper;

    public GetNotificationsQueryHandler(IRepository<Notification> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var query = _repository.Query().AsQueryable();

        if (request.IsRead.HasValue)
            query = query.Where(n => n.IsRead == request.IsRead.Value);

        query = query.OrderByDescending(n => n.CreatedAt);

        return await PaginatedList<NotificationDto>.CreateAsync(
            query.ProjectToType<NotificationDto>(_mapper.Config),
            request.PageNumber,
            request.PageSize,
            ct);
    }
}
