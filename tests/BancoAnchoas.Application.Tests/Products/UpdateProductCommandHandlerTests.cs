using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Products.Commands.UpdateProduct;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private UpdateProductCommandHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    public UpdateProductCommandHandlerTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProductFields()
    {
        var product = new Product { Id = 1, Name = "Harina", Sku = "PROD-00001", Unit = "kg", Stock = 10, CategoryId = 1 };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var command = new UpdateProductCommand(1, "Harina Integral", "Descripción", "1234567890123", 5.50m, "kg", 3, null, "Proveedor X", 2, null);
        await CreateHandler().Handle(command, CancellationToken.None);

        product.Name.Should().Be("Harina Integral");
        product.Description.Should().Be("Descripción");
        product.Barcode.Should().Be("1234567890123");
        product.Price.Should().Be(5.50m);
        product.CategoryId.Should().Be(2);
        product.Supplier.Should().Be("Proveedor X");
        product.Sku.Should().Be("PROD-00001", "Sku should not be changed by update");
        _repoMock.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenProductNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand(999, "X", null, null, null, "kg", 0, null, null, 1, null);

        await FluentActions.Invoking(() => CreateHandler().Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
