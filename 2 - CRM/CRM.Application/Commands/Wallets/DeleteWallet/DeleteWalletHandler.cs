using MediatR;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Wallets.DeleteWallet;

public sealed class DeleteWalletHandler : IRequestHandler<DeleteWalletCommand, Result>
{
    private readonly IWalletRepository _repository;

    public DeleteWalletHandler(IWalletRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeleteWalletCommand request, CancellationToken ct)
    {
        var wallet = await _repository.GetByIdAsync(request.Id, ct);
        if (wallet is null)
            return Result.Failure("WALLET_NOT_FOUND", "Wallet not found.");

        wallet.SoftDelete();
        _repository.Update(wallet);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
