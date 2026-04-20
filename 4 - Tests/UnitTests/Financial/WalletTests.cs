using MyCRM.CRM.Application.Commands.Wallets.CreateWallet;
using MyCRM.CRM.Application.Commands.Transactions.CreateTransaction;
using MyCRM.CRM.Application.Commands.Transactions.UpdateTransaction;
using MyCRM.CRM.Application.Commands.Transactions.DeleteTransaction;
using MyCRM.CRM.Application.Commands.Transactions.CreateTransfer;
using MyCRM.CRM.Application.Commands.Transactions.ReconcileTransaction;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.Financial;

public sealed class WalletTests
{
    private readonly IWalletRepository       _wallets        = Substitute.For<IWalletRepository>();
    private readonly ITransactionRepository  _transactions   = Substitute.For<ITransactionRepository>();
    private readonly IPaymentMethodRepository _paymentMethods = Substitute.For<IPaymentMethodRepository>();
    private readonly ICategoryRepository     _categories     = Substitute.For<ICategoryRepository>();
    private readonly IReconciliationRepository _reconciliations = Substitute.For<IReconciliationRepository>();
    private readonly ITenantService          _tenant         = Substitute.For<ITenantService>();
    private readonly Guid _tenantId = Guid.NewGuid();

    public WalletTests()
    {
        _tenant.TenantId.Returns(_tenantId);
    }

    // CT-008: Criar carteira com saldo inicial
    [Fact]
    public async Task CreateWallet_WithInitialBalance_ReturnsSuccess()
    {
        _wallets.SaveChangesAsync(default).Returns(1);

        var handler = new CreateWalletHandler(_wallets, _tenant);
        var result = await handler.Handle(new CreateWalletCommand("Carteira Principal", 1000m), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Carteira Principal", result.Value!.Name);
        Assert.Equal(1000m, result.Value!.InitialBalance);
        Assert.True(result.Value!.IsActive);
    }

    // CT-001: Registrar entrada PIX com categoria
    [Fact]
    public async Task CreateTransaction_Income_ReturnsSuccess()
    {
        var wallet = Wallet.Create(_tenantId, "Caixa", 0);
        var paymentMethod = PaymentMethod.Create(_tenantId, "PIX", Guid.NewGuid());
        var category = Category.Create(_tenantId, "Receita");
        _wallets.GetByIdAsync(wallet.Id, default).Returns(wallet);
        _paymentMethods.GetByIdAsync(paymentMethod.Id, default).Returns(paymentMethod);
        _categories.GetByIdAsync(category.Id, default).Returns(category);
        _transactions.SaveChangesAsync(default).Returns(1);

        var handler = new CreateTransactionHandler(_transactions, _wallets, _paymentMethods, _categories, _tenant);
        var result = await handler.Handle(new CreateTransactionCommand(
            wallet.Id, TransactionType.Income, 500m, "Mensalidade",
            category.Id, paymentMethod.Id, DateTime.Today), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionType.Income, result.Value!.Type);
        Assert.Equal(500m, result.Value!.Amount);
    }

    // CT-002: Registrar saída em dinheiro
    [Fact]
    public async Task CreateTransaction_Expense_ReturnsSuccess()
    {
        var wallet = Wallet.Create(_tenantId, "Caixa", 1000m);
        var paymentMethod = PaymentMethod.Create(_tenantId, "Dinheiro", Guid.NewGuid());
        var category = Category.Create(_tenantId, "Despesa");
        _wallets.GetByIdAsync(wallet.Id, default).Returns(wallet);
        _paymentMethods.GetByIdAsync(paymentMethod.Id, default).Returns(paymentMethod);
        _categories.GetByIdAsync(category.Id, default).Returns(category);
        _transactions.SaveChangesAsync(default).Returns(1);

        var handler = new CreateTransactionHandler(_transactions, _wallets, _paymentMethods, _categories, _tenant);
        var result = await handler.Handle(new CreateTransactionCommand(
            wallet.Id, TransactionType.Expense, 200m, "Aluguel",
            category.Id, paymentMethod.Id, DateTime.Today), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionType.Expense, result.Value!.Type);
        Assert.Equal(200m, result.Value!.Amount);
    }

    // CT-004: Editar transação não reconciliada
    [Fact]
    public async Task UpdateTransaction_NotReconciled_ReturnsSuccess()
    {
        var transaction = Transaction.Create(
            _tenantId, Guid.NewGuid(), TransactionType.Income,
            100m, "Desc", Guid.NewGuid(), Guid.NewGuid(), DateTime.Today);

        _transactions.GetByIdAsync(transaction.Id, default).Returns(transaction);
        _transactions.SaveChangesAsync(default).Returns(1);

        var handler = new UpdateTransactionHandler(_transactions);
        var result = await handler.Handle(new UpdateTransactionCommand(
            transaction.Id, 200m, "Desc Atualizada",
            Guid.NewGuid(), Guid.NewGuid(), DateTime.Today), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(200m, result.Value!.Amount);
    }

    // CT-005: Tentar editar transação reconciliada (deve falhar)
    [Fact]
    public async Task UpdateTransaction_Reconciled_ReturnsFailure()
    {
        var transaction = Transaction.Create(
            _tenantId, Guid.NewGuid(), TransactionType.Income,
            100m, "Desc", Guid.NewGuid(), Guid.NewGuid(), DateTime.Today);
        transaction.MarkAsReconciled();

        _transactions.GetByIdAsync(transaction.Id, default).Returns(transaction);

        var handler = new UpdateTransactionHandler(_transactions);
        var result = await handler.Handle(new UpdateTransactionCommand(
            transaction.Id, 200m, "Nova", Guid.NewGuid(), Guid.NewGuid(), DateTime.Today), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRANSACTION_RECONCILED", result.ErrorCode);
        await _transactions.DidNotReceive().SaveChangesAsync(default);
    }

    // CT-007: Tentar excluir transação reconciliada (deve falhar)
    [Fact]
    public async Task DeleteTransaction_Reconciled_ReturnsFailure()
    {
        var transaction = Transaction.Create(
            _tenantId, Guid.NewGuid(), TransactionType.Income,
            100m, "Desc", Guid.NewGuid(), Guid.NewGuid(), DateTime.Today);
        transaction.MarkAsReconciled();

        _transactions.GetByIdAsync(transaction.Id, default).Returns(transaction);

        var handler = new DeleteTransactionHandler(_transactions);
        var result = await handler.Handle(new DeleteTransactionCommand(transaction.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRANSACTION_RECONCILED", result.ErrorCode);
    }

    // CT-012: Excluir transação não reconciliada
    [Fact]
    public async Task DeleteTransaction_NotReconciled_ReturnsSuccess()
    {
        var transaction = Transaction.Create(
            _tenantId, Guid.NewGuid(), TransactionType.Income,
            100m, "Desc", Guid.NewGuid(), Guid.NewGuid(), DateTime.Today);

        _transactions.GetByIdAsync(transaction.Id, default).Returns(transaction);
        _transactions.SaveChangesAsync(default).Returns(1);

        var handler = new DeleteTransactionHandler(_transactions);
        var result = await handler.Handle(new DeleteTransactionCommand(transaction.Id), default);

        Assert.True(result.IsSuccess);
        Assert.True(transaction.IsDeleted);
    }

    // CT-006: Conciliar transação
    [Fact]
    public async Task ReconcileTransaction_ValidTransaction_ReturnsSuccess()
    {
        var transaction = Transaction.Create(
            _tenantId, Guid.NewGuid(), TransactionType.Income,
            500m, "Desc", Guid.NewGuid(), Guid.NewGuid(), DateTime.Today);

        _transactions.GetByIdAsync(transaction.Id, default).Returns(transaction);
        _reconciliations.SaveChangesAsync(default).Returns(1);

        var handler = new ReconcileTransactionHandler(_transactions, _reconciliations, _tenant);
        var result = await handler.Handle(new ReconcileTransactionCommand(
            transaction.Id, "EXT-001", 500m, DateTime.Today), default);

        Assert.True(result.IsSuccess);
        Assert.True(transaction.IsReconciled);
        Assert.Equal("EXT-001", result.Value!.ExternalId);
    }

    // CT-003: Transferir entre carteiras (gera 2 transactions)
    [Fact]
    public async Task CreateTransfer_ValidWallets_CreatesTwoTransactions()
    {
        var fromWallet = Wallet.Create(_tenantId, "Conta Principal", 1000m);
        var toWallet   = Wallet.Create(_tenantId, "Conta Poupança", 0m);

        _wallets.GetByIdAsync(fromWallet.Id, default).Returns(fromWallet);
        _wallets.GetByIdAsync(toWallet.Id, default).Returns(toWallet);
        _wallets.GetCurrentBalanceAsync(fromWallet.Id, default).Returns(1000m);
        _transactions.SaveChangesAsync(default).Returns(1);

        var handler = new CreateTransferHandler(_transactions, _wallets, _tenant);
        var result = await handler.Handle(new CreateTransferCommand(
            fromWallet.Id, toWallet.Id, 300m, "Transferência",
            Guid.NewGuid(), Guid.NewGuid(), DateTime.Today), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(TransactionType.Expense, result.Value!.ExpenseTransaction.Type);
        Assert.Equal(TransactionType.Income,  result.Value!.IncomeTransaction.Type);
        Assert.Equal(300m, result.Value!.ExpenseTransaction.Amount);
        Assert.Equal(fromWallet.Id, result.Value!.ExpenseTransaction.WalletId);
        Assert.Equal(toWallet.Id,   result.Value!.IncomeTransaction.WalletId);
    }

    // CT-009: Transferência com saldo insuficiente (deve falhar)
    [Fact]
    public async Task CreateTransfer_InsufficientBalance_ReturnsFailure()
    {
        var fromWallet = Wallet.Create(_tenantId, "Conta Vazia", 0m);
        var toWallet   = Wallet.Create(_tenantId, "Destino", 0m);

        _wallets.GetByIdAsync(fromWallet.Id, default).Returns(fromWallet);
        _wallets.GetByIdAsync(toWallet.Id, default).Returns(toWallet);
        _wallets.GetCurrentBalanceAsync(fromWallet.Id, default).Returns(50m);

        var handler = new CreateTransferHandler(_transactions, _wallets, _tenant);
        var result = await handler.Handle(new CreateTransferCommand(
            fromWallet.Id, toWallet.Id, 500m, "Transferência",
            Guid.NewGuid(), Guid.NewGuid(), DateTime.Today), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("INSUFFICIENT_BALANCE", result.ErrorCode);
    }

    // CT-011: Registrar transação com PaymentMethod inativo (deve falhar — PM não encontrado)
    [Fact]
    public async Task CreateTransaction_PaymentMethodNotFound_ReturnsFailure()
    {
        var wallet = Wallet.Create(_tenantId, "Caixa", 1000m);
        _wallets.GetByIdAsync(wallet.Id, default).Returns(wallet);
        _paymentMethods.GetByIdAsync(Arg.Any<Guid>(), default).Returns((PaymentMethod?)null);

        var handler = new CreateTransactionHandler(_transactions, _wallets, _paymentMethods, _categories, _tenant);
        var result = await handler.Handle(new CreateTransactionCommand(
            wallet.Id, TransactionType.Income, 100m, "Desc",
            Guid.NewGuid(), Guid.NewGuid(), DateTime.Today), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("PAYMENT_METHOD_NOT_FOUND", result.ErrorCode);
    }

    // CT-LAC014: Transferência com carteira de origem inativa (deve falhar)
    [Fact]
    public async Task CreateTransfer_InactiveSourceWallet_ReturnsFailure()
    {
        var fromWallet = Wallet.Create(_tenantId, "Conta Inativa", 1000m);
        fromWallet.SetInactive();
        var toWallet = Wallet.Create(_tenantId, "Destino", 0m);

        _wallets.GetByIdAsync(fromWallet.Id, default).Returns(fromWallet);
        _wallets.GetByIdAsync(toWallet.Id, default).Returns(toWallet);

        var handler = new CreateTransferHandler(_transactions, _wallets, _tenant);
        var result = await handler.Handle(new CreateTransferCommand(
            fromWallet.Id, toWallet.Id, 300m, "Transferência",
            Guid.NewGuid(), Guid.NewGuid(), DateTime.Today), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("WALLET_INACTIVE", result.ErrorCode);
        await _transactions.DidNotReceive().SaveChangesAsync(default);
    }

    // CT-LAC015: Registrar transação com CategoryId inexistente (deve falhar)
    [Fact]
    public async Task CreateTransaction_CategoryNotFound_ReturnsFailure()
    {
        var wallet = Wallet.Create(_tenantId, "Caixa", 1000m);
        var paymentMethod = PaymentMethod.Create(_tenantId, "PIX", Guid.NewGuid());
        _wallets.GetByIdAsync(wallet.Id, default).Returns(wallet);
        _paymentMethods.GetByIdAsync(paymentMethod.Id, default).Returns(paymentMethod);
        _categories.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Category?)null);

        var handler = new CreateTransactionHandler(_transactions, _wallets, _paymentMethods, _categories, _tenant);
        var result = await handler.Handle(new CreateTransactionCommand(
            wallet.Id, TransactionType.Income, 100m, "Desc",
            Guid.NewGuid(), paymentMethod.Id, DateTime.Today), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("CATEGORY_NOT_FOUND", result.ErrorCode);
        await _transactions.DidNotReceive().SaveChangesAsync(default);
    }

    // CT-010: Listar transações por período (domain rule test — não precisa de handler, testa a entidade)
    [Fact]
    public void Transaction_Create_SetsCorrectProperties()
    {
        var walletId = Guid.NewGuid();
        var date     = new DateTime(2026, 1, 15);

        var tx = Transaction.Create(
            _tenantId, walletId, TransactionType.Income,
            750m, "Mensalidade Fevereiro", Guid.NewGuid(), Guid.NewGuid(), date);

        Assert.Equal(walletId,              tx.WalletId);
        Assert.Equal(TransactionType.Income, tx.Type);
        Assert.Equal(750m,                  tx.Amount);
        Assert.Equal(date,                  tx.TransactionDate);
        Assert.False(tx.IsReconciled);
        Assert.False(tx.IsDeleted);
    }
}
