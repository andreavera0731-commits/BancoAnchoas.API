using MediatR;

namespace BancoAnchoas.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string? Description) : IRequest<int>;
