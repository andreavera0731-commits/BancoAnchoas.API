using BancoAnchoas.Application.Features.Stock.Commands.RegisterRelocation;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Stock;

public class RegisterRelocationCommandValidatorTests
{
    private readonly RegisterRelocationCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_WithValidCommand()
    {
        var command = new RegisterRelocationCommand(1, 1, 2, 5, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenProductIdIsZero()
    {
        var command = new RegisterRelocationCommand(0, 1, 2, 5, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ProductId);
    }

    [Fact]
    public void Should_Fail_WhenQuantityIsZero()
    {
        var command = new RegisterRelocationCommand(1, 1, 2, 0, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Fact]
    public void Should_Fail_WhenFromAndToSectorAreSame()
    {
        var command = new RegisterRelocationCommand(1, 1, 1, 5, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Should_Fail_WhenFromSectorIdIsZero()
    {
        var command = new RegisterRelocationCommand(1, 0, 2, 5, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.FromSectorId);
    }

    [Fact]
    public void Should_Fail_WhenSectorIdIsZero()
    {
        var command = new RegisterRelocationCommand(1, 1, 0, 5, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SectorId);
    }
}
