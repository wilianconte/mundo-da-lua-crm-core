using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.CancelTenantPlan;

public record CancelTenantPlanCommand(
    Guid TenantId,
    Guid DowngradeToPlanId
) : IRequest<Result>;
