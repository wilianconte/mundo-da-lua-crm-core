using HotChocolate.Authorization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Application.Queries.GetAllTransactions;
using MyCRM.CRM.Application.Queries.GetTransactionById;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;

namespace MyCRM.GraphQL.GraphQL.Financial;

[QueryType]
public sealed class TransactionQueries
{
    [Authorize(Policy = SystemPermissions.TransactionsRead)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Transaction> GetTransactions([Service] CRMDbContext db) =>
        db.Transactions.AsNoTracking();

    [Authorize(Policy = SystemPermissions.TransactionsRead)]
    public async Task<TransactionDto?> GetTransactionByIdAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id), ct);
        return result.IsSuccess ? result.Value : null;
    }

    [Authorize(Policy = SystemPermissions.TransactionsRead)]
    public async Task<IReadOnlyList<TransactionDto>> GetTransactionsFilteredAsync(
        Guid? walletId,
        DateTime? startDate,
        DateTime? endDate,
        TransactionType? type,
        Guid? categoryId,
        Guid? paymentMethodId,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetAllTransactionsQuery(walletId, startDate, endDate, type, categoryId, paymentMethodId), ct);
        return result.IsSuccess ? result.Value! : [];
    }
}
