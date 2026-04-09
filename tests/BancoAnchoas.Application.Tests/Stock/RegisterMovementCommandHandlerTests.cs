using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterMovement;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Stock;

public class RegisterMovementCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IRepository<StockMovement>> _movementRepoMock = new();
    private readonly Mock<IRepository<Requester>> _requesterRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private RegisterMovementCommandHandler CreateHandler() =>
        new(_productRepoMock.Object, _movementRepoMock.Object, _requesterRepoMock.Object, _uowMock.Object, _currentUserMock.Object);

    public RegisterMovementCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns("user-1");
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_Entry_ShouldIncreaseStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterMovementCommand(1, 1, 5, MovementType.Entry, null);
        await CreateHandler().Handle(command, CancellationToken.None);

        product.Stock.Should().Be(15);
    }

    [Fact]
    public async Task Handle_Exit_ShouldDecreaseStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _requesterRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Requester { Id = 1, Name = "Cocina" });

        var command = new RegisterMovementCommand(1, 1, 3, MovementType.Exit, null, RequesterId: 1);
        await CreateHandler().Handle(command, CancellationToken.None);

        product.Stock.Should().Be(7);
    }

    [Fact]
    public async Task Handle_Exit_ShouldThrow_WhenQuantityExceedsStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 2, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _requesterRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Requester { Id = 1, Name = "Cocina" });

        var command = new RegisterMovementCommand(1, 1, 10, MovementType.Exit, null, RequesterId: 1);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_Exit_ShouldThrow_WhenRequesterNotFound()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _requesterRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Requester?)null);

        var command = new RegisterMovementCommand(1, 1, 3, MovementType.Exit, null, RequesterId: 999);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNotFound()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new RegisterMovementCommand(999, 1, 5, MovementType.Entry, null);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
