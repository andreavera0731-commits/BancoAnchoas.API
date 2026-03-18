using BancoAnchoas.Application.Features.Products.Commands.CreateProduct;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Products;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_WithValidCommand()
    {
        var command = new CreateProductCommand("Harina", null, null, null, "kg", 10, 5, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenNameIsEmpty()
    {
        var command = new CreateProductCommand("", null, null, null, "kg", 0, 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Should_Fail_WhenUnitIsInvalid()
    {
        var command = new CreateProductCommand("Harina", null, null, null, "boxes", 0, 0, null, null, 1, null);
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
        var command = new CreateProductCommand("Test", null, null, null, unit, 0, 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Unit);
    }

    [Fact]
    public void Should_Fail_WhenStockIsNegative()
    {
        var command = new CreateProductCommand("Harina", null, null, null, "kg", -1, 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Stock);
    }

    [Fact]
    public void Should_Fail_WhenCategoryIdIsZero()
    {
        var command = new CreateProductCommand("Harina", null, null, null, "kg", 0, 0, null, null, 0, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
    }

    [Fact]
    public void Should_Fail_WhenPriceIsNegative()
    {
        var command = new CreateProductCommand("Harina", null, null, -1m, "kg", 0, 0, null, null, 1, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Price);
    }
}
