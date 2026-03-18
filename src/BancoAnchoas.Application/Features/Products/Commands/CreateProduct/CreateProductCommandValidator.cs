using FluentValidation;

namespace BancoAnchoas.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private static readonly string[] ValidUnits = ["kg", "g", "un", "lt", "ml"];

    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20)
            .Must(u => ValidUnits.Contains(u.ToLowerInvariant()))
            .WithMessage("Unit must be one of: kg, g, un, lt, ml");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.Barcode).MaximumLength(100).When(x => x.Barcode is not null);
    }
}
