using BancoAnchoas.Application.Features.Products.Commands.UpdateProduct;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Products;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_WithValidCommand()
    {
        var command = new UpdateProductCommand(1, "Harina", null, null, null, "kg", 5, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenIdIsZero()
    {
        var command = new UpdateProductCommand(0, "Harina", null, null, null, "kg", 5, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Should_Fail_WhenNameIsEmpty()
    {
        var command = new UpdateProductCommand(1, "", null, null, null, "kg", 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Should_Fail_WhenUnitIsInvalid()
    {
        var command = new UpdateProductCommand(1, "Harina", null, null, null, "boxes", 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Unit);
    }

    [Theory]
    [InlineData("kg")]
    [InlineData("g")]
    [InlineData("un")]
    [InlineData("lt")]
    [InlineData("ml")]
    public void Should_Pass_WithValidUnit(string unit)
    {
        var command = new UpdateProductCommand(1, "Test", null, null, null, unit, 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Unit);
    }

    [Fact]
    public void Should_Fail_WhenPriceIsNegative()
    {
        var command = new UpdateProductCommand(1, "Harina", null, null, -1m, "kg", 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void Should_Fail_WhenCategoryIdIsZero()
    {
        var command = new UpdateProductCommand(1, "Harina", null, null, null, "kg", 0, null, null, 0, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public void Should_Fail_WhenMinimumStockIsNegative()
    {
        var command = new UpdateProductCommand(1, "Harina", null, null, null, "kg", -1, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.MinimumStock);
    }

    [Fact]
    public void Should_Fail_WhenBarcodeExceedsMaxLength()
    {
        var command = new UpdateProductCommand(1, "Harina", null, new string('X', 101), null, "kg", 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Barcode);
    }
}
