using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Transactions.DeleteTransaction;

public record DeleteTransactionCommand(Guid Id) : IRequest<Result>;
