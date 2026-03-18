using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Categories.Commands.CreateCategory;
using BancoAnchoas.Application.Features.Categories.Commands.DeactivateCategory;
using BancoAnchoas.Application.Features.Categories.Commands.UpdateCategory;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Categories;

public class CategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public CategoryCommandHandlerTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // --- CreateCategory ---

    [Fact]
    public async Task Create_ShouldReturnId()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Returns<Category, CancellationToken>((c, _) => { c.Id = 5; return Task.FromResult(c); });

        var handler = new CreateCategoryCommandHandler(_repoMock.Object, _uowMock.Object);
        var result = await handler.Handle(new CreateCategoryCommand("Harinas", "Harinas de trigo"), CancellationToken.None);

        result.Should().Be(5);
    }

    // --- UpdateCategory ---

    [Fact]
    public async Task Update_ShouldModifyFields()
    {
        var category = new Category { Id = 1, Name = "Harinas" };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var handler = new UpdateCategoryCommandHandler(_repoMock.Object, _uowMock.Object);
        await handler.Handle(new UpdateCategoryCommand(1, "Lácteos", "Leche y derivados"), CancellationToken.None);

        category.Name.Should().Be("Lácteos");
        category.Description.Should().Be("Leche y derivados");
    }

    [Fact]
    public async Task Update_ShouldThrow_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var handler = new UpdateCategoryCommandHandler(_repoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new UpdateCategoryCommand(999, "X", null), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    // --- DeactivateCategory ---

    [Fact]
    public async Task Deactivate_ShouldSoftDelete()
    {
        var category = new Category { Id = 1, Name = "Harinas", IsActive = true };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var handler = new DeactivateCategoryCommandHandler(_repoMock.Object, _uowMock.Object);
        await handler.Handle(new DeactivateCategoryCommand(1), CancellationToken.None);

        category.IsActive.Should().BeFalse();
        category.DeactivatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Deactivate_ShouldThrow_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var handler = new DeactivateCategoryCommandHandler(_repoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new DeactivateCategoryCommand(999), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
