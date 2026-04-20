using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Professionals.CreateProfessional;

public sealed class CreateProfessionalValidator : AbstractValidator<CreateProfessionalCommand>
{
    public CreateProfessionalValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty().WithMessage("PersonId is required.");

        RuleFor(x => x.SpecialtyIds)
            .NotEmpty().WithMessage("At least one specialty is required (RN-046).");

        RuleFor(x => x.CommissionPercentage)
            .InclusiveBetween(0, 100).WithMessage("CommissionPercentage must be between 0 and 100.")
            .When(x => x.CommissionPercentage.HasValue);

        RuleFor(x => x.Bio).MaximumLength(2000).When(x => x.Bio is not null);
        RuleFor(x => x.LicenseNumber).MaximumLength(100).When(x => x.LicenseNumber is not null);
    }
}
