using FluentValidation;

namespace BancoAnchoas.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private static readonly string[] ValidUnits = ["kg", "g", "un", "lt", "ml"];

    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20)
            .Must(u => ValidUnits.Contains(u.ToLowerInvariant()))
            .WithMessage("Unit must be one of: kg, g, un, lt, ml");
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.Barcode).MaximumLength(100).When(x => x.Barcode is not null);
    }
}
