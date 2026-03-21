using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.DeactivateCustomer;

public record DeactivateCustomerCommand(Guid Id) : IRequest<Result>;
