using MediatR;
using MyCRM.CRM.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.PaymentMethods.CreatePaymentMethod;

public record CreatePaymentMethodCommand(string Name) : IRequest<Result<PaymentMethodDto>>;
