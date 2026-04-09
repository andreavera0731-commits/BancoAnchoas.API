using FluentValidation;

namespace BancoAnchoas.Application.Features.Requesters.Commands.CreateRequester;

public class CreateRequesterCommandValidator : AbstractValidator<CreateRequesterCommand>
{
    public CreateRequesterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
