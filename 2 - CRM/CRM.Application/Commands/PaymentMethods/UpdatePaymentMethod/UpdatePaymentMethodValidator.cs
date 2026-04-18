using FluentValidation;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.UpdatePaymentMethod;

public sealed class UpdatePaymentMethodValidator : AbstractValidator<UpdatePaymentMethodCommand>
{
    public UpdatePaymentMethodValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
