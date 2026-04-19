using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<decimal> GetCurrentBalanceAsync(Guid walletId, CancellationToken ct = default);
}
