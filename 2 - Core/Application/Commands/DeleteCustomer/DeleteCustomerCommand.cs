using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Application.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<Result>;
