using BancoAnchoas.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace BancoAnchoas.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(string Email, string Name, string Password, string Role) : IRequest<string>;

public class CreateUserCommandValidator : FluentValidation.AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "Admin" or "Almacenista")
            .WithMessage("Role must be Admin or Almacenista.");
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
        => _identityService = identityService;

    public async Task<string> Handle(CreateUserCommand request, CancellationToken ct)
    {
        return await _identityService.CreateUserAsync(request.Email, request.Name, request.Password, request.Role);
    }
}
