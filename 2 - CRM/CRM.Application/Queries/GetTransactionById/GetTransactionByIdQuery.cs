using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid Id) : IRequest<Result<TransactionDto?>>;
