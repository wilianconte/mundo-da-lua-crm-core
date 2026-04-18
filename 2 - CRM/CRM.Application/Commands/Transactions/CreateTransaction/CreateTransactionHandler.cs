using Mapster;
using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.CreateTransaction;

public sealed class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, Result<TransactionDto>>
{
    private readonly ITransactionRepository  _transactions;
    private readonly IWalletRepository       _wallets;
    private readonly IPaymentMethodRepository _paymentMethods;
    private readonly ITenantService          _tenant;

    public CreateTransactionHandler(
        ITransactionRepository  transactions,
        IWalletRepository       wallets,
        IPaymentMethodRepository paymentMethods,
        ITenantService          tenant)
    {
        _transactions   = transactions;
        _wallets        = wallets;
        _paymentMethods = paymentMethods;
        _tenant         = tenant;
    }

    public async Task<Result<TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        var wallet = await _wallets.GetByIdAsync(request.WalletId, ct);
        if (wallet is null)
            return Result<TransactionDto>.Failure("WALLET_NOT_FOUND", "Wallet not found.");

        if (!wallet.IsActive)
            return Result<TransactionDto>.Failure("WALLET_INACTIVE", "Wallet is inactive.");

        var paymentMethod = await _paymentMethods.GetByIdAsync(request.PaymentMethodId, ct);
        if (paymentMethod is null)
            return Result<TransactionDto>.Failure("PAYMENT_METHOD_NOT_FOUND", "Payment method not found.");

        var transaction = Transaction.Create(
            _tenant.TenantId,
            request.WalletId,
            request.Type,
            request.Amount,
            request.Description,
            request.CategoryId,
            request.PaymentMethodId,
            request.TransactionDate);

        await _transactions.AddAsync(transaction, ct);
        await _transactions.SaveChangesAsync(ct);

        return Result<TransactionDto>.Success(transaction.Adapt<TransactionDto>());
    }
}
