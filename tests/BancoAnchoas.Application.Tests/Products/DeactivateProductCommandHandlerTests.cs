using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Products.Commands.DeactivateProduct;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Products;

public class DeactivateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private DeactivateProductCommandHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    public DeactivateProductCommandHandlerTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteProduct()
    {
        var product = new Product { Id = 1, Name = "Harina", Sku = "PROD-00001", Unit = "kg", Stock = 10, IsActive = true, CategoryId = 1 };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await CreateHandler().Handle(new DeactivateProductCommand(1), CancellationToken.None);

        product.IsActive.Should().BeFalse();
        product.DeactivatedAt.Should().NotBeNull();
        product.UpdatedAt.Should().NotBeNull();
        _repoMock.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        await FluentActions.Invoking(() => CreateHandler().Handle(new DeactivateProductCommand(999), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
