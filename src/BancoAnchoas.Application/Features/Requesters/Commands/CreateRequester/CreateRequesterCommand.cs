using MediatR;

namespace BancoAnchoas.Application.Features.Requesters.Commands.CreateRequester;

public record CreateRequesterCommand(string Name, string? Description) : IRequest<int>;
