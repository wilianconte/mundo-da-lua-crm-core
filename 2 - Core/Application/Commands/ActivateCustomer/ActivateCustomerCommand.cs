using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Application.Commands.ActivateCustomer;

public record ActivateCustomerCommand(Guid Id) : IRequest<Result>;
