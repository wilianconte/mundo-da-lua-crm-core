using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Transactions.CreateTransaction;

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.WalletId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PaymentMethodId).NotEmpty();
    }
}
