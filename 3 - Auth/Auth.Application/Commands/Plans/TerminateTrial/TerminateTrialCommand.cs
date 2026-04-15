using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Plans.TerminateTrial;

/// <param name="TenantId">Id do tenant.</param>
/// <param name="DowngradeToPlanId">
/// Plano de destino após o trial. Obrigatório quando não há plano pausado.
/// Se nulo e não houver plano pausado, o handler retornará erro.
/// </param>
public record TerminateTrialCommand(
    Guid  TenantId,
    Guid? DowngradeToPlanId
) : IRequest<Result>;
