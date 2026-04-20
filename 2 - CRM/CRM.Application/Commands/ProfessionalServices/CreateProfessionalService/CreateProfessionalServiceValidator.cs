using FluentValidation;

namespace MyCRM.CRM.Application.Commands.ProfessionalServices.CreateProfessionalService;

public sealed class CreateProfessionalServiceValidator : AbstractValidator<CreateProfessionalServiceCommand>
{
    public CreateProfessionalServiceValidator()
    {
        RuleFor(x => x.ProfessionalId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.CustomPrice).GreaterThan(0).When(x => x.CustomPrice.HasValue)
            .WithMessage("CustomPrice must be greater than zero.");
        RuleFor(x => x.CustomDurationInMinutes).GreaterThan(0).When(x => x.CustomDurationInMinutes.HasValue)
            .WithMessage("CustomDurationInMinutes must be greater than zero.");
    }
}
