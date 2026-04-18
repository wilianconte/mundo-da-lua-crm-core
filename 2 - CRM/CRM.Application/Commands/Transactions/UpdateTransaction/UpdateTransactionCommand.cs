using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.UpdateTransaction;

public record UpdateTransactionCommand(
    Guid     Id,
    decimal  Amount,
    string   Description,
    Guid     CategoryId,
    Guid     PaymentMethodId,
    DateTime TransactionDate) : IRequest<Result<TransactionDto>>;
