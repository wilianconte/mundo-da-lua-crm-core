using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Customers.DeactivateCustomer;

public record DeactivateCustomerCommand(Guid Id) : IRequest<Result>;
