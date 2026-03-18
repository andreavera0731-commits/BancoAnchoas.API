using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BancoAnchoas.Application.Features.Stock.Commands.RegisterAdjustment;

public record RegisterAdjustmentCommand(
    int ProductId,
    int SectorId,
    int Quantity,
    AdjustmentType AdjustmentType,
    MovementReason? Reason,
    string? Notes) : IRequest<int>;

public class RegisterAdjustmentCommandValidator : FluentValidation.AbstractValidator<RegisterAdjustmentCommand>
{
    public RegisterAdjustmentCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.SectorId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.AdjustmentType).IsInEnum();
    }
}

public class RegisterAdjustmentCommandHandler : IRequestHandler<RegisterAdjustmentCommand, int>
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<StockMovement> _movementRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RegisterAdjustmentCommandHandler(
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

    public async Task<int> Handle(RegisterAdjustmentCommand request, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        if (request.AdjustmentType == AdjustmentType.Increase)
        {
            product.Stock += request.Quantity;
        }
        else
        {
            if (request.Quantity > product.Stock)
                throw new Common.Exceptions.ValidationException(
                    new[] { new FluentValidation.Results.ValidationFailure("Quantity", "Quantity exceeds available stock.") });
            product.Stock -= request.Quantity;
        }

        product.UpdatedAt = DateTime.UtcNow;
        _productRepo.Update(product);

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            SectorId = request.SectorId,
            Quantity = request.Quantity,
            Type = MovementType.Adjustment,
            AdjustmentType = request.AdjustmentType,
            Reason = request.Reason,
            Notes = request.Notes,
            UserId = _currentUser.UserId ?? string.Empty
        };

        await _movementRepo.AddAsync(movement, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return movement.Id;
    }
}
