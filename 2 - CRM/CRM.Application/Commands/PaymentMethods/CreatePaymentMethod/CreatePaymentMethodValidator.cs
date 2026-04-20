using FluentValidation;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.CreatePaymentMethod;

public sealed class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WalletId).NotEmpty();
    }
}
