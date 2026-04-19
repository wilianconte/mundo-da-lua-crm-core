using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Wallets.UpdateWallet;

public sealed class UpdateWalletHandler : IRequestHandler<UpdateWalletCommand, Result<WalletDto>>
{
    private readonly IWalletRepository _repository;

    public UpdateWalletHandler(IWalletRepository repository) => _repository = repository;

    public async Task<Result<WalletDto>> Handle(UpdateWalletCommand request, CancellationToken ct)
    {
        var wallet = await _repository.GetByIdAsync(request.Id, ct);
        if (wallet is null)
            return Result<WalletDto>.Failure("WALLET_NOT_FOUND", "Wallet not found.");

        wallet.Update(request.Name, request.InitialBalance);
        _repository.Update(wallet);

        var balance = await _repository.GetCurrentBalanceAsync(wallet.Id, ct);
        await _repository.SaveChangesAsync(ct);

        var dto = new WalletDto(
            wallet.Id, wallet.TenantId, wallet.Name,
            balance, wallet.InitialBalance, wallet.IsActive,
            wallet.CreatedAt, wallet.UpdatedAt);

        return Result<WalletDto>.Success(dto);
    }
}
