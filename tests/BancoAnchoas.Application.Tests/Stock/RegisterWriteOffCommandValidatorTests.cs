using BancoAnchoas.Application.Features.Stock.Commands.RegisterWriteOff;
using BancoAnchoas.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Stock;

public class RegisterWriteOffCommandValidatorTests
{
    private readonly RegisterWriteOffCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_WithValidCommand()
    {
        var command = new RegisterWriteOffCommand(1, 1, 5, MovementReason.Damage, "Se regaron");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenProductIdIsZero()
    {
        var command = new RegisterWriteOffCommand(0, 1, 5, MovementReason.Damage, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ProductId);
    }

    [Fact]
    public void Should_Fail_WhenSectorIdIsZero()
    {
        var command = new RegisterWriteOffCommand(1, 0, 5, MovementReason.Damage, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.SectorId);
    }

    [Fact]
    public void Should_Fail_WhenQuantityIsZero()
    {
        var command = new RegisterWriteOffCommand(1, 1, 0, MovementReason.Damage, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Fact]
    public void Should_Fail_WhenReasonIsInvalid()
    {
        var command = new RegisterWriteOffCommand(1, 1, 5, (MovementReason)99, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Reason);
    }
}
