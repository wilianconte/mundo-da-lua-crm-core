using FluentValidation;

namespace MyCRM.CRM.Application.Commands.SetCustomerAddress;

public sealed class SetCustomerAddressValidator : AbstractValidator<SetCustomerAddressCommand>
{
    public SetCustomerAddressValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Street).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Number).MaximumLength(20).When(x => x.Number is not null);
        RuleFor(x => x.Complement).MaximumLength(100).When(x => x.Complement is not null);
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(150);
        RuleFor(x => x.City).NotEmpty().MaximumLength(150);
        RuleFor(x => x.State).NotEmpty().Length(2).WithMessage("State must be a 2-letter code.");
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Country).NotEmpty().Length(2).WithMessage("Country must be a 2-letter ISO code.");
    }
}
