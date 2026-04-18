using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.CreateTransaction;

public record CreateTransactionCommand(
    Guid            WalletId,
    TransactionType Type,
    decimal         Amount,
    string          Description,
    Guid            CategoryId,
    Guid            PaymentMethodId,
    DateTime        TransactionDate) : IRequest<Result<TransactionDto>>;
