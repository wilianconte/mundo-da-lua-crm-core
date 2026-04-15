using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.StartTrial;

public record StartTrialCommand(
    Guid TenantId,
    Guid TrialPlanId
) : IRequest<Result>;
