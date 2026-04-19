using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Transactions.CreateTransfer;

public sealed class CreateTransferValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferValidator()
    {
        RuleFor(x => x.FromWalletId).NotEmpty();
        RuleFor(x => x.ToWalletId).NotEmpty()
            .NotEqual(x => x.FromWalletId).WithMessage("Source and destination wallets must be different.");
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PaymentMethodId).NotEmpty();
    }
}
