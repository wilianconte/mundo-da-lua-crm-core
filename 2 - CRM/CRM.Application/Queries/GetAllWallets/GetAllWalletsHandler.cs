using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllWallets;

public sealed class GetAllWalletsHandler : IRequestHandler<GetAllWalletsQuery, Result<IReadOnlyList<WalletDto>>>
{
    private readonly IWalletRepository       _wallets;
    private readonly ITransactionRepository  _transactions;

    public GetAllWalletsHandler(IWalletRepository wallets, ITransactionRepository transactions)
    {
        _wallets      = wallets;
        _transactions = transactions;
    }

    public async Task<Result<IReadOnlyList<WalletDto>>> Handle(GetAllWalletsQuery request, CancellationToken ct)
    {
        var wallets = await _wallets.GetAllAsync(ct);

        var dtos = new List<WalletDto>();
        foreach (var wallet in wallets)
        {
            var balance = await _wallets.GetCurrentBalanceAsync(wallet.Id, ct);
            dtos.Add(new WalletDto(
                wallet.Id, wallet.TenantId, wallet.Name,
                balance, wallet.InitialBalance, wallet.IsActive,
                wallet.CreatedAt, wallet.UpdatedAt));
        }

        return Result<IReadOnlyList<WalletDto>>.Success(dtos);
    }
}
