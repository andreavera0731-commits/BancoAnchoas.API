using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Interfaces;
using BancoAnchoas.Application.Features.Stock.Commands.RegisterRelocation;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Enums;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Stock;

public class RegisterRelocationCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IRepository<StockMovement>> _movementRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private RegisterRelocationCommandHandler CreateHandler() =>
        new(_productRepoMock.Object, _movementRepoMock.Object, _uowMock.Object, _currentUserMock.Object);

    public RegisterRelocationCommandHandlerTests()
    {
        _currentUserMock.Setup(c => c.UserId).Returns("user-1");
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldNotModifyStock()
    {
        var product = new Product { Id = 1, Name = "Harina", Stock = 10, Unit = "kg", Sku = "PROD-00001", CategoryId = 1 };
        _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new RegisterRelocationCommand(1, 1, 2, 5, null);
        await CreateHandler().Handle(command, CancellationToken.None);

        product.Stock.Should().Be(10, "relocation should not change total stock");
        _movementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(m =>
            m.Type == MovementType.Relocation &&
            m.FromSectorId == 1 &&
            m.SectorId == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNotFound()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new RegisterRelocationCommand(999, 1, 2, 5, null);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
