using MyCRM.CRM.Application.DTOs;

namespace MyCRM.GraphQL.GraphQL.Financial;

public record TransferPayload(TransactionDto ExpenseTransaction, TransactionDto IncomeTransaction);
