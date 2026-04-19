using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Wallets.CreateWallet;

public sealed class CreateWalletValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}
