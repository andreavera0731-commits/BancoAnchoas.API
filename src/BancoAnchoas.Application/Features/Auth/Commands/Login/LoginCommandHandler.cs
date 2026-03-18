using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Auth.DTOs;
using MediatR;

namespace BancoAnchoas.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IIdentityService identityService, IJwtTokenService jwtTokenService)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var auth = await _identityService.AuthenticateAsync(request.Email, request.Password)
            ?? throw new ForbiddenException("Invalid credentials.");

        var token = _jwtTokenService.GenerateToken(auth.UserId, auth.Email, auth.Name, auth.Role);

        return new LoginResponseDto(token, new UserInfoDto(auth.UserId, auth.Email, auth.Name, auth.Role));
    }
}
