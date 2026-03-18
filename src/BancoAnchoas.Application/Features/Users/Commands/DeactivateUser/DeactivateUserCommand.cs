using BancoAnchoas.Application.Common.Interfaces;
using MediatR;

namespace BancoAnchoas.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(string Id) : IRequest;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IIdentityService _identityService;

    public DeactivateUserCommandHandler(IIdentityService identityService)
        => _identityService = identityService;

    public async Task Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        await _identityService.DeactivateUserAsync(request.Id);
    }
}
