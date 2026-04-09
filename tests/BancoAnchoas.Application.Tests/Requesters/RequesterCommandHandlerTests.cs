using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Requesters.Commands.CreateRequester;
using BancoAnchoas.Application.Features.Requesters.Commands.DeactivateRequester;
using BancoAnchoas.Application.Features.Requesters.Commands.UpdateRequester;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Requesters;

public class RequesterCommandHandlerTests
{
    private readonly Mock<IRepository<Requester>> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public RequesterCommandHandlerTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // --- CreateRequester ---

    [Fact]
    public async Task Create_ShouldReturnId()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Requester>(), It.IsAny<CancellationToken>()))
            .Returns<Requester, CancellationToken>((r, _) => { r.Id = 5; return Task.FromResult(r); });

        var handler = new CreateRequesterCommandHandler(_repoMock.Object, _uowMock.Object);
        var result = await handler.Handle(new CreateRequesterCommand("Cocina", "Sector cocina"), CancellationToken.None);

        result.Should().Be(5);
    }

    // --- UpdateRequester ---

    [Fact]
    public async Task Update_ShouldModifyFields()
    {
        var requester = new Requester { Id = 1, Name = "Cocina" };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(requester);

        var handler = new UpdateRequesterCommandHandler(_repoMock.Object, _uowMock.Object);
        await handler.Handle(new UpdateRequesterCommand(1, "Panadería", "Sector panadería"), CancellationToken.None);

        requester.Name.Should().Be("Panadería");
        requester.Description.Should().Be("Sector panadería");
    }

    [Fact]
    public async Task Update_ShouldThrow_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Requester?)null);

        var handler = new UpdateRequesterCommandHandler(_repoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new UpdateRequesterCommand(999, "X", null), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    // --- DeactivateRequester ---

    [Fact]
    public async Task Deactivate_ShouldSoftDelete()
    {
        var requester = new Requester { Id = 1, Name = "Cocina", IsActive = true };
        _repoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(requester);

        var handler = new DeactivateRequesterCommandHandler(_repoMock.Object, _uowMock.Object);
        await handler.Handle(new DeactivateRequesterCommand(1), CancellationToken.None);

        requester.IsActive.Should().BeFalse();
        requester.DeactivatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Deactivate_ShouldThrow_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Requester?)null);

        var handler = new DeactivateRequesterCommandHandler(_repoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new DeactivateRequesterCommand(999), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
