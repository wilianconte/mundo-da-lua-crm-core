using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.CreateTransfer;

public sealed class CreateTransferHandler : IRequestHandler<CreateTransferCommand, Result<TransferResultDto>>
{
    private readonly ITransactionRepository  _transactions;
    private readonly IWalletRepository       _wallets;
    private readonly ITenantService          _tenant;

    public CreateTransferHandler(
        ITransactionRepository  transactions,
        IWalletRepository       wallets,
        ITenantService          tenant)
    {
        _transactions = transactions;
        _wallets      = wallets;
        _tenant       = tenant;
    }

    public async Task<Result<TransferResultDto>> Handle(CreateTransferCommand request, CancellationToken ct)
    {
        var fromWallet = await _wallets.GetByIdAsync(request.FromWalletId, ct);
        if (fromWallet is null)
            return Result<TransferResultDto>.Failure("WALLET_NOT_FOUND", "Source wallet not found.");

        var toWallet = await _wallets.GetByIdAsync(request.ToWalletId, ct);
        if (toWallet is null)
            return Result<TransferResultDto>.Failure("WALLET_NOT_FOUND", "Destination wallet not found.");

        var currentBalance = await _wallets.GetCurrentBalanceAsync(request.FromWalletId, ct);
        if (currentBalance < request.Amount)
            return Result<TransferResultDto>.Failure("INSUFFICIENT_BALANCE", "Insufficient balance in source wallet.");

        var expense = Transaction.Create(
            _tenant.TenantId,
            request.FromWalletId,
            TransactionType.Expense,
            request.Amount,
            request.Description,
            request.CategoryId,
            request.PaymentMethodId,
            request.TransactionDate);

        var income = Transaction.Create(
            _tenant.TenantId,
            request.ToWalletId,
            TransactionType.Income,
            request.Amount,
            request.Description,
            request.CategoryId,
            request.PaymentMethodId,
            request.TransactionDate);

        await _transactions.AddAsync(expense, ct);
        await _transactions.AddAsync(income, ct);
        await _transactions.SaveChangesAsync(ct);

        var result = new TransferResultDto(
            expense.Adapt<TransactionDto>(),
            income.Adapt<TransactionDto>());

        return Result<TransferResultDto>.Success(result);
    }
}
