using BancoAnchoas.Application.Features.Categories.Commands.CreateCategory;
using BancoAnchoas.Application.Features.Categories.Commands.UpdateCategory;
using FluentValidation.TestHelper;

namespace BancoAnchoas.Application.Tests.Categories;

public class CategoryCommandValidatorTests
{
    // --- CreateCategoryCommandValidator ---

    private readonly CreateCategoryCommandValidator _createValidator = new();

    [Fact]
    public void Create_Should_Pass_WithValidCommand()
    {
        var result = _createValidator.TestValidate(new CreateCategoryCommand("Harinas", null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_Should_Fail_WhenNameIsEmpty()
    {
        var result = _createValidator.TestValidate(new CreateCategoryCommand("", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Create_Should_Fail_WhenNameExceedsMaxLength()
    {
        var result = _createValidator.TestValidate(new CreateCategoryCommand(new string('X', 101), null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    // --- UpdateCategoryCommandValidator ---

    private readonly UpdateCategoryCommandValidator _updateValidator = new();

    [Fact]
    public void Update_Should_Pass_WithValidCommand()
    {
        var result = _updateValidator.TestValidate(new UpdateCategoryCommand(1, "Harinas", null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_Should_Fail_WhenIdIsZero()
    {
        var result = _updateValidator.TestValidate(new UpdateCategoryCommand(0, "Harinas", null));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Update_Should_Fail_WhenNameIsEmpty()
    {
        var result = _updateValidator.TestValidate(new UpdateCategoryCommand(1, "", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Update_Should_Fail_WhenNameExceedsMaxLength()
    {
        var result = _updateValidator.TestValidate(new UpdateCategoryCommand(1, new string('X', 101), null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}
