using MediatR;

namespace BancoAnchoas.Application.Features.Requesters.Commands.UpdateRequester;

public record UpdateRequesterCommand(int Id, string Name, string? Description) : IRequest;
