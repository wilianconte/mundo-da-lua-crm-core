using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.RevertCancellation;

public record RevertCancellationCommand(Guid TenantId) : IRequest<Result>;
