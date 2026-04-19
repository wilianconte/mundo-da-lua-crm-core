using FluentValidation;

namespace MyCRM.CRM.Application.Commands.Wallets.UpdateWallet;

public sealed class UpdateWalletValidator : AbstractValidator<UpdateWalletCommand>
{
    public UpdateWalletValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}
