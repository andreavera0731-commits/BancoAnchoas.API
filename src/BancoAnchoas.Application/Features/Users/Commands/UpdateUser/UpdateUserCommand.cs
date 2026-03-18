using BancoAnchoas.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace BancoAnchoas.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(string Id, string? Name, string? Email, string? Role) : IRequest;

public class UpdateUserCommandValidator : FluentValidation.AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email is not null);
        RuleFor(x => x.Role).Must(r => r is "Admin" or "Almacenista").When(x => x.Role is not null)
            .WithMessage("Role must be Admin or Almacenista.");
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IIdentityService _identityService;

    public UpdateUserCommandHandler(IIdentityService identityService)
        => _identityService = identityService;

    public async Task Handle(UpdateUserCommand request, CancellationToken ct)
    {
        await _identityService.UpdateUserAsync(request.Id, request.Name, request.Email, request.Role);
    }
}
