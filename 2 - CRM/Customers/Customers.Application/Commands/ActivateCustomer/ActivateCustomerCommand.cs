using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Customers.Application.Commands.ActivateCustomer;

public record ActivateCustomerCommand(Guid Id) : IRequest<Result>;
