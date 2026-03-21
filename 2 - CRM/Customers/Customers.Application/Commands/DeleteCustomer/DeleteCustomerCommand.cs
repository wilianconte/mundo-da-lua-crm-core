using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Customers.Application.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<Result>;
