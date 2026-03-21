using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Customers.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<Result>;
