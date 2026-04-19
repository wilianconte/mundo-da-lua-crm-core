using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.UpdatePaymentMethod;

public record UpdatePaymentMethodCommand(Guid Id, string Name) : IRequest<Result<PaymentMethodDto>>;
