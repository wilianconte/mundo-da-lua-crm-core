using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.UpgradeTenantPlan;

public record UpgradeTenantPlanCommand(
    Guid TenantId,
    Guid NewPlanId
) : IRequest<Result>;
