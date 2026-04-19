using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllPaymentMethods;

public record GetAllPaymentMethodsQuery : IRequest<Result<IReadOnlyList<PaymentMethodDto>>>;
