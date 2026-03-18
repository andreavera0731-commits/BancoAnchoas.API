using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Users.DTOs;
using MediatR;

namespace BancoAnchoas.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
        => _identityService = identityService;

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var users = await _identityService.GetUsersAsync();
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            Role = u.Role,
            IsActive = u.IsActive
        }).ToList();
    }
}
