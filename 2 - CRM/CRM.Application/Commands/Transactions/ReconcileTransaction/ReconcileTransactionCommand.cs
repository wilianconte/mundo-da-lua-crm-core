using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.ReconcileTransaction;

public record ReconcileTransactionCommand(
    Guid     TransactionId,
    string   ExternalId,
    decimal  ExternalAmount,
    DateTime ExternalDate) : IRequest<Result<ReconciliationDto>>;
