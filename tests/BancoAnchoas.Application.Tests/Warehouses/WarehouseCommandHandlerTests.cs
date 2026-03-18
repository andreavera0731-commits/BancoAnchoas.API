using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Features.Warehouses.Commands.CreateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.CreateWarehouse;
using BancoAnchoas.Application.Features.Warehouses.Commands.DeactivateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.DeactivateWarehouse;
using BancoAnchoas.Application.Features.Warehouses.Commands.UpdateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.UpdateWarehouse;
using BancoAnchoas.Domain.Entities;
using BancoAnchoas.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BancoAnchoas.Application.Tests.Warehouses;

public class WarehouseCommandHandlerTests
{
    private readonly Mock<IRepository<Warehouse>> _warehouseRepoMock = new();
    private readonly Mock<IRepository<Sector>> _sectorRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    public WarehouseCommandHandlerTests()
    {
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // --- CreateWarehouse ---

    [Fact]
    public async Task CreateWarehouse_ShouldReturnId()
    {
        _warehouseRepoMock.Setup(r => r.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()))
            .Returns<Warehouse, CancellationToken>((w, _) => { w.Id = 3; return Task.FromResult(w); });

        var handler = new CreateWarehouseCommandHandler(_warehouseRepoMock.Object, _uowMock.Object);
        var result = await handler.Handle(new CreateWarehouseCommand("Almacén Central", "Calle 123"), CancellationToken.None);

        result.Should().Be(3);
    }

    // --- UpdateWarehouse ---

    [Fact]
    public async Task UpdateWarehouse_ShouldModifyFields()
    {
        var warehouse = new Warehouse { Id = 1, Name = "Almacén Central" };
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

        var handler = new UpdateWarehouseCommandHandler(_warehouseRepoMock.Object, _uowMock.Object);
        await handler.Handle(new UpdateWarehouseCommand(1, "Depósito Norte", "Av. Norte 456"), CancellationToken.None);

        warehouse.Name.Should().Be("Depósito Norte");
        warehouse.Location.Should().Be("Av. Norte 456");
    }

    [Fact]
    public async Task UpdateWarehouse_ShouldThrow_WhenNotFound()
    {
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var handler = new UpdateWarehouseCommandHandler(_warehouseRepoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new UpdateWarehouseCommand(999, "X", null), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    // --- DeactivateWarehouse ---

    [Fact]
    public async Task DeactivateWarehouse_ShouldThrow_WhenHasActiveSectors()
    {
        var warehouse = new Warehouse { Id = 1, Name = "Almacén Central" };
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

        var sectors = new List<Sector> { new() { Id = 1, Name = "Panadería", WarehouseId = 1 } }.AsQueryable();
        _sectorRepoMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Sector>(sectors));

        var handler = new DeactivateWarehouseCommandHandler(_warehouseRepoMock.Object, _sectorRepoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new DeactivateWarehouseCommand(1), CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeactivateWarehouse_ShouldSoftDelete_WhenNoActiveSectors()
    {
        var warehouse = new Warehouse { Id = 1, Name = "Almacén Central", IsActive = true };
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

        var empty = Enumerable.Empty<Sector>().AsQueryable();
        _sectorRepoMock.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Sector>(empty));

        var handler = new DeactivateWarehouseCommandHandler(_warehouseRepoMock.Object, _sectorRepoMock.Object, _uowMock.Object);
        await handler.Handle(new DeactivateWarehouseCommand(1), CancellationToken.None);

        warehouse.IsActive.Should().BeFalse();
        warehouse.DeactivatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeactivateWarehouse_ShouldThrow_WhenNotFound()
    {
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var handler = new DeactivateWarehouseCommandHandler(_warehouseRepoMock.Object, _sectorRepoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new DeactivateWarehouseCommand(999), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    // --- CreateSector ---

    [Fact]
    public async Task CreateSector_ShouldReturnId()
    {
        _sectorRepoMock.Setup(r => r.AddAsync(It.IsAny<Sector>(), It.IsAny<CancellationToken>()))
            .Returns<Sector, CancellationToken>((s, _) => { s.Id = 7; return Task.FromResult(s); });

        var handler = new CreateSectorCommandHandler(_sectorRepoMock.Object, _uowMock.Object);
        var result = await handler.Handle(new CreateSectorCommand("Panadería", 1), CancellationToken.None);

        result.Should().Be(7);
    }

    // --- UpdateSector ---

    [Fact]
    public async Task UpdateSector_ShouldModifyFields()
    {
        var sector = new Sector { Id = 1, Name = "Panadería", WarehouseId = 1 };
        _sectorRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sector);

        var handler = new UpdateSectorCommandHandler(_sectorRepoMock.Object, _uowMock.Object);
        await handler.Handle(new UpdateSectorCommand(1, "Chocolatería"), CancellationToken.None);

        sector.Name.Should().Be("Chocolatería");
    }

    [Fact]
    public async Task UpdateSector_ShouldThrow_WhenNotFound()
    {
        _sectorRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Sector?)null);

        var handler = new UpdateSectorCommandHandler(_sectorRepoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new UpdateSectorCommand(999, "X"), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    // --- DeactivateSector ---

    [Fact]
    public async Task DeactivateSector_ShouldSoftDelete()
    {
        var sector = new Sector { Id = 1, Name = "Panadería", WarehouseId = 1, IsActive = true };
        _sectorRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sector);

        var handler = new DeactivateSectorCommandHandler(_sectorRepoMock.Object, _uowMock.Object);
        await handler.Handle(new DeactivateSectorCommand(1), CancellationToken.None);

        sector.IsActive.Should().BeFalse();
        sector.DeactivatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeactivateSector_ShouldThrow_WhenNotFound()
    {
        _sectorRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Sector?)null);

        var handler = new DeactivateSectorCommandHandler(_sectorRepoMock.Object, _uowMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new DeactivateSectorCommand(999), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
