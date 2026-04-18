using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.DeletePaymentMethod;

public record DeletePaymentMethodCommand(Guid Id) : IRequest<Result>;
