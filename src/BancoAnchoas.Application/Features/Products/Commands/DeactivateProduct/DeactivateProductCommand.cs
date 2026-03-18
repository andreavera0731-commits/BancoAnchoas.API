using MediatR;

namespace BancoAnchoas.Application.Features.Products.Commands.DeactivateProduct;

public record DeactivateProductCommand(int Id) : IRequest;
