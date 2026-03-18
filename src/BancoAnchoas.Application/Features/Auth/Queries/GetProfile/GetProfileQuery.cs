using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Auth.DTOs;
using MediatR;

namespace BancoAnchoas.Application.Features.Auth.Queries.GetProfile;

public record GetProfileQuery : IRequest<UserProfileDto>;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUser;

    public GetProfileQueryHandler(IIdentityService identityService, ICurrentUserService currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserProfileDto> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenException("User not authenticated.");

        var user = await _identityService.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        return new UserProfileDto(user.Id, user.Email, user.Name, user.Role);
    }
}
