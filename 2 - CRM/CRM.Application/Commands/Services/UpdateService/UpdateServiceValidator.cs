using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Services.UpdateService;

public sealed class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DefaultPrice).GreaterThan(0).WithMessage("DefaultPrice must be greater than zero (RN-047).");
        RuleFor(x => x.DefaultDurationInMinutes).GreaterThan(0).WithMessage("DefaultDurationInMinutes must be greater than zero (RN-047).");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
