using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllTransactions;

public record GetAllTransactionsQuery(
    Guid?            WalletId,
    DateTime?        StartDate,
    DateTime?        EndDate,
    TransactionType? Type,
    Guid?            CategoryId,
    Guid?            PaymentMethodId) : IRequest<Result<IReadOnlyList<TransactionDto>>>;
