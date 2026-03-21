using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Application.Commands.DeactivateCustomer;

public record DeactivateCustomerCommand(Guid Id) : IRequest<Result>;
