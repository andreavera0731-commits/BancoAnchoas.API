using BancoAnchoas.Application.Features.Requesters.Commands.CreateRequester;
using BancoAnchoas.Application.Features.Requesters.Commands.UpdateRequester;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Requesters;

public class RequesterCommandValidatorTests
{
    // --- CreateRequesterCommandValidator ---

    private readonly CreateRequesterCommandValidator _createValidator = new();

    [Fact]
    public void Create_Should_Pass_WithValidCommand()
    {
        var result = _createValidator.TestValidate(new CreateRequesterCommand("Cocina", null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_Should_Fail_WhenNameIsEmpty()
    {
        var result = _createValidator.TestValidate(new CreateRequesterCommand("", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Create_Should_Fail_WhenNameExceedsMaxLength()
    {
        var result = _createValidator.TestValidate(new CreateRequesterCommand(new string('X', 101), null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    // --- UpdateRequesterCommandValidator ---

    private readonly UpdateRequesterCommandValidator _updateValidator = new();

    [Fact]
    public void Update_Should_Pass_WithValidCommand()
    {
        var result = _updateValidator.TestValidate(new UpdateRequesterCommand(1, "Cocina", null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_Should_Fail_WhenIdIsZero()
    {
        var result = _updateValidator.TestValidate(new UpdateRequesterCommand(0, "Cocina", null));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Update_Should_Fail_WhenNameIsEmpty()
    {
        var result = _updateValidator.TestValidate(new UpdateRequesterCommand(1, "", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Update_Should_Fail_WhenNameExceedsMaxLength()
    {
        var result = _updateValidator.TestValidate(new UpdateRequesterCommand(1, new string('X', 101), null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}
