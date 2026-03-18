using BancoAnchoas.Application.Features.Warehouses.Commands.CreateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.CreateWarehouse;
using BancoAnchoas.Application.Features.Warehouses.Commands.UpdateSector;
using BancoAnchoas.Application.Features.Warehouses.Commands.UpdateWarehouse;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Warehouses;

public class WarehouseCommandValidatorTests
{
    // --- CreateWarehouseCommandValidator ---

    private readonly CreateWarehouseCommandValidator _createWarehouseValidator = new();

    [Fact]
    public void CreateWarehouse_Should_Pass_WithValidCommand()
    {
        var result = _createWarehouseValidator.TestValidate(new CreateWarehouseCommand("Almacén Central", "Calle 123"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateWarehouse_Should_Fail_WhenNameIsEmpty()
    {
        var result = _createWarehouseValidator.TestValidate(new CreateWarehouseCommand("", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void CreateWarehouse_Should_Fail_WhenNameExceedsMaxLength()
    {
        var result = _createWarehouseValidator.TestValidate(new CreateWarehouseCommand(new string('X', 101), null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void CreateWarehouse_Should_Fail_WhenLocationExceedsMaxLength()
    {
        var result = _createWarehouseValidator.TestValidate(new CreateWarehouseCommand("Almacén", new string('X', 201)));
        result.ShouldHaveValidationErrorFor(c => c.Location);
    }

    // --- UpdateWarehouseCommandValidator ---

    private readonly UpdateWarehouseCommandValidator _updateWarehouseValidator = new();

    [Fact]
    public void UpdateWarehouse_Should_Pass_WithValidCommand()
    {
        var result = _updateWarehouseValidator.TestValidate(new UpdateWarehouseCommand(1, "Almacén", null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateWarehouse_Should_Fail_WhenIdIsZero()
    {
        var result = _updateWarehouseValidator.TestValidate(new UpdateWarehouseCommand(0, "Almacén", null));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void UpdateWarehouse_Should_Fail_WhenNameIsEmpty()
    {
        var result = _updateWarehouseValidator.TestValidate(new UpdateWarehouseCommand(1, "", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    // --- CreateSectorCommandValidator ---

    private readonly CreateSectorCommandValidator _createSectorValidator = new();

    [Fact]
    public void CreateSector_Should_Pass_WithValidCommand()
    {
        var result = _createSectorValidator.TestValidate(new CreateSectorCommand("Panadería", 1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateSector_Should_Fail_WhenNameIsEmpty()
    {
        var result = _createSectorValidator.TestValidate(new CreateSectorCommand("", 1));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void CreateSector_Should_Fail_WhenWarehouseIdIsZero()
    {
        var result = _createSectorValidator.TestValidate(new CreateSectorCommand("Panadería", 0));
        result.ShouldHaveValidationErrorFor(c => c.WarehouseId);
    }

    // --- UpdateSectorCommandValidator ---

    private readonly UpdateSectorCommandValidator _updateSectorValidator = new();

    [Fact]
    public void UpdateSector_Should_Pass_WithValidCommand()
    {
        var result = _updateSectorValidator.TestValidate(new UpdateSectorCommand(1, "Chocolatería"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateSector_Should_Fail_WhenIdIsZero()
    {
        var result = _updateSectorValidator.TestValidate(new UpdateSectorCommand(0, "Chocolatería"));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void UpdateSector_Should_Fail_WhenNameIsEmpty()
    {
        var result = _updateSectorValidator.TestValidate(new UpdateSectorCommand(1, ""));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}
