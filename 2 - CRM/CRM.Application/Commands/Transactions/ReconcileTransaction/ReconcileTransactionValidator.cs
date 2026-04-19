using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Transactions.ReconcileTransaction;

public sealed class ReconcileTransactionValidator : AbstractValidator<ReconcileTransactionCommand>
{
    public ReconcileTransactionValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.ExternalId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExternalAmount).GreaterThan(0);
    }
}
