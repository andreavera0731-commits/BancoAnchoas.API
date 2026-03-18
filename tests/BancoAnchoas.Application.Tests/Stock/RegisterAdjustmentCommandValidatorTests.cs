using BancoAnchoas.Application.Features.Stock.Commands.RegisterAdjustment;
using BancoAnchoas.Domain.Enums;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Stock;

public class RegisterAdjustmentCommandValidatorTests
{
    private readonly RegisterAdjustmentCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_WithValidCommand()
    {
        var command = new RegisterAdjustmentCommand(1, 1, 5, AdjustmentType.Increase, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenProductIdIsZero()
    {
        var command = new RegisterAdjustmentCommand(0, 1, 5, AdjustmentType.Increase, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ProductId);
    }

    [Fact]
    public void Should_Fail_WhenSectorIdIsZero()
    {
        var command = new RegisterAdjustmentCommand(1, 0, 5, AdjustmentType.Increase, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SectorId);
    }

    [Fact]
    public void Should_Fail_WhenQuantityIsZero()
    {
        var command = new RegisterAdjustmentCommand(1, 1, 0, AdjustmentType.Decrease, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Fact]
    public void Should_Fail_WhenAdjustmentTypeIsInvalid()
    {
        var command = new RegisterAdjustmentCommand(1, 1, 5, (AdjustmentType)99, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.AdjustmentType);
    }
}
