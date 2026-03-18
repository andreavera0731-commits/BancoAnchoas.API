using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterWriteOff;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Stock;

public class RegisterWriteOffCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IRepository<StockMovement>> _movementRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private RegisterWriteOffCommandHandler CreateHandler() =>
        new(_productRepoMock.Object, _movementRepoMock.Object, _uowMock.Object, _currentUserMock.Object);

    public RegisterWriteOffCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns("user-1");
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldDecreaseStockAndCreateMovement()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterWriteOffCommand(1, 1, 3, MovementReason.Damage, "Se regaron en el depósito");
        await CreateHandler().Handle(command, CancellationToken.None);

        product.Stock.Should().Be(7);
        _movementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(m =>
            m.Type == MovementType.WriteOff &&
            m.Reason == MovementReason.Damage &&
            m.Quantity == 3), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenQuantityExceedsStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 2, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterWriteOffCommand(1, 1, 10, MovementReason.Loss, null);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNotFound()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new RegisterWriteOffCommand(999, 1, 5, MovementReason.Expiration, null);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
