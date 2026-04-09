using FluentValidation;

namespace BancoAnchoas.Application.Features.Reports.Queries.ExportMovements;

public class ExportMovementsQueryValidator : AbstractValidator<ExportMovementsQuery>
{
    public ExportMovementsQueryValidator()
    {
        RuleFor(x => x.Format).IsInEnum();
        RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId.HasValue);
        RuleFor(x => x.SectorId).GreaterThan(0).When(x => x.SectorId.HasValue);
        RuleFor(x => x.RequesterId).GreaterThan(0).When(x => x.RequesterId.HasValue);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("La fecha 'to' debe ser posterior o igual a 'from'.");
    }
}
