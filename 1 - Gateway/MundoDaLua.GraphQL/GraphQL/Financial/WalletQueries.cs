using HotChocolate.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Application.Queries.GetAllWallets;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[QueryType]
public sealed class WalletQueries
{
    [Authorize(Policy = SystemPermissions.WalletsRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Wallet> GetWallets([Service] CRMDbContext db) =>
        db.Wallets.AsNoTracking();

    [Authorize(Policy = SystemPermissions.WalletsRead)]
    public async Task<IReadOnlyList<WalletDto>> GetWalletsWithBalance(
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetAllWalletsQuery(), ct);
        return result.IsSuccess ? result.Value! : [];
    }
}
