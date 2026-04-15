using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.MarkBillingAsPaid;

public record MarkBillingAsPaidCommand(
    Guid BillingId,
    Guid TenantId
) : IRequest<Result>;
