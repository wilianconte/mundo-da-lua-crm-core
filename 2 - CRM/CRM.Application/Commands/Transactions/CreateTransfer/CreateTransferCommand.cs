using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.CreateTransfer;

public record CreateTransferCommand(
    Guid     FromWalletId,
    Guid     ToWalletId,
    decimal  Amount,
    string   Description,
    Guid     CategoryId,
    Guid     PaymentMethodId,
    DateTime TransactionDate) : IRequest<Result<TransferResultDto>>;

public record TransferResultDto(TransactionDto ExpenseTransaction, TransactionDto IncomeTransaction);
