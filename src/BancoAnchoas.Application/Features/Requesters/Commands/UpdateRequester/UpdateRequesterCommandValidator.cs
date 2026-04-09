using FluentValidation;

namespace BancoAnchoas.Application.Features.Requesters.Commands.UpdateRequester;

public class UpdateRequesterCommandValidator : AbstractValidator<UpdateRequesterCommand>
{
    public UpdateRequesterCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
