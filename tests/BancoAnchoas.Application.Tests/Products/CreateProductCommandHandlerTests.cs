using BancoAnchoas.Application.Features.Products.Commands.CreateProduct;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CreateProductCommandHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_ShouldCreateProductAndGenerateSku()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Harina", null, null, null, "kg", 10, 5, null, null, 1, null);

        Product? capturedProduct = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns<Product, CancellationToken>((p, _) =>
            {
                p.Id = 42;
                capturedProduct = p;
                return Task.FromResult(p);
            });

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(42);
        capturedProduct!.Sku.Should().Be("PROD-00042");
        capturedProduct.Name.Should().Be("Harina");
        capturedProduct.Unit.Should().Be("kg");
        _repoMock.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
