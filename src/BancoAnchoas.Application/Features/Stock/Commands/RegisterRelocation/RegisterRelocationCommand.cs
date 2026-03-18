using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BancoAnchoas.Application.Features.Stock.Commands.RegisterRelocation;

public record RegisterRelocationCommand(
    int ProductId,
    int FromSectorId,
    int SectorId,
    int Quantity,
    string? Notes) : IRequest<int>;

public class RegisterRelocationCommandValidator : FluentValidation.AbstractValidator<RegisterRelocationCommand>
{
    public RegisterRelocationCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.FromSectorId).GreaterThan(0);
        RuleFor(x => x.SectorId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x).Must(x => x.FromSectorId != x.SectorId)
            .WithMessage("Source and destination sectors must be different.");
    }
}

public class RegisterRelocationCommandHandler : IRequestHandler<RegisterRelocationCommand, int>
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<StockMovement> _movementRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RegisterRelocationCommandHandler(
        IRepository<Product> productRepo,
        IRepository<StockMovement> movementRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _productRepo = productRepo;
        _movementRepo = movementRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(RegisterRelocationCommand request, CancellationToken ct)
    {
        // Verify product exists (active)
        _ = await _productRepo.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        // Relocation does NOT modify Product.Stock (total unchanged)
        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            SectorId = request.SectorId,
            FromSectorId = request.FromSectorId,
            Quantity = request.Quantity,
            Type = MovementType.Relocation,
            Notes = request.Notes,
            UserId = _currentUser.UserId ?? string.Empty
        };

        await _movementRepo.AddAsync(movement, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return movement.Id;
    }
}
