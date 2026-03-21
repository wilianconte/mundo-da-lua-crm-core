using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Customers.ActivateCustomer;

public record ActivateCustomerCommand(Guid Id) : IRequest<Result>;
