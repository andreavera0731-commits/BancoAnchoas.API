using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterAdjustment;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Stock;

public class RegisterAdjustmentCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IRepository<StockMovement>> _movementRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private RegisterAdjustmentCommandHandler CreateHandler() =>
        new(_productRepoMock.Object, _movementRepoMock.Object, _uowMock.Object, _currentUserMock.Object);

    public RegisterAdjustmentCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns("user-1");
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_Increase_ShouldAddToStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterAdjustmentCommand(1, 1, 5, AdjustmentType.Increase, null, null);
        await CreateHandler().Handle(command, CancellationToken.None);

        product.Stock.Should().Be(15);
    }

    [Fact]
    public async Task Handle_Decrease_ShouldSubtractFromStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterAdjustmentCommand(1, 1, 3, AdjustmentType.Decrease, MovementReason.Loss, "Faltante en conteo");
        await CreateHandler().Handle(command, CancellationToken.None);

        product.Stock.Should().Be(7);
    }

    [Fact]
    public async Task Handle_Decrease_ShouldThrow_WhenQuantityExceedsStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 2, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterAdjustmentCommand(1, 1, 10, AdjustmentType.Decrease, null, null);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNotFound()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new RegisterAdjustmentCommand(999, 1, 5, AdjustmentType.Increase, null, null);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldCreateMovementWithAdjustmentType()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterAdjustmentCommand(1, 1, 5, AdjustmentType.Increase, MovementReason.Other, "Inventario físico");
        await CreateHandler().Handle(command, CancellationToken.None);

        _movementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(m =>
            m.Type == MovementType.Adjustment &&
            m.AdjustmentType == AdjustmentType.Increase &&
            m.Reason == MovementReason.Other), It.IsAny<CancellationToken>()), Times.Once);
    }
}
