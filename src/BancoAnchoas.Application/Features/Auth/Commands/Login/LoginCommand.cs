using MediatR;
using BancoAnchoas.Application.Features.Auth.DTOs;

namespace BancoAnchoas.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDto>;
