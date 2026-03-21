using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Customers.Application.Commands.DeactivateCustomer;

public record DeactivateCustomerCommand(Guid Id) : IRequest<Result>;
