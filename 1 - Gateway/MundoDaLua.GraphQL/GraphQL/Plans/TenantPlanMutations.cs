using MediatR;
using MyCRM.Auth.Application.Commands.Plans.CancelTenantPlan;
using MyCRM.Auth.Application.Commands.Plans.MarkBillingAsPaid;
using MyCRM.Auth.Application.Commands.Plans.RevertCancellation;
using MyCRM.Auth.Application.Commands.Plans.StartTrial;
using MyCRM.Auth.Application.Commands.Plans.TerminateTrial;
using MyCRM.Auth.Application.Commands.Plans.UpgradeTenantPlan;
using MyCRM.GraphQL.GraphQL.Plans.Inputs;
using MyCRM.Shared.Kernel;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.GraphQL.GraphQL.Plans;

[MutationType]
public sealed class TenantPlanMutations
{
    /// <summary>
    /// Realiza upgrade do plano ativo do tenant autenticado para um novo plano.
    /// </summary>
    [Authorize(Policy = SystemPermissions.PlansManage)]
    public async Task<bool> UpgradeTenantPlanAsync(
        UpgradeTenantPlanInput input,
        [Service] ISender sender,
        [Service] ITenantService tenantService,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpgradeTenantPlanCommand(tenantService.TenantId, input.NewPlanId), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    /// <summary>
    /// Solicita cancelamento do plano ativo ao final do período vigente.
    /// O tenant fará downgrade para o plano especificado quando o plano atual expirar.
    /// </summary>
    [Authorize(Policy = SystemPermissions.PlansManage)]
    public async Task<bool> CancelTenantPlanAsync(
        CancelTenantPlanInput input,
        [Service] ISender sender,
        [Service] ITenantService tenantService,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CancelTenantPlanCommand(tenantService.TenantId, input.DowngradeToPlanId), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    /// <summary>
    /// Reverte o cancelamento pendente do plano ativo do tenant autenticado.
    /// </summary>
    [Authorize(Policy = SystemPermissions.PlansManage)]
    public async Task<bool> RevertCancellationAsync(
        [Service] ISender sender,
        [Service] ITenantService tenantService,
        CancellationToken ct)
    {
        var result = await sender.Send(new RevertCancellationCommand(tenantService.TenantId), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    /// <summary>
    /// Inicia um período de trial para um plano pago.
    /// O plano atual é pausado (se pago) ou expirado (se Free).
    /// </summary>
    [Authorize(Policy = SystemPermissions.PlansManage)]
    public async Task<bool> StartTrialAsync(
        StartTrialInput input,
        [Service] ISender sender,
        [Service] ITenantService tenantService,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new StartTrialCommand(tenantService.TenantId, input.TrialPlanId), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    /// <summary>
    /// Encerra o período de trial. Retoma o plano pausado (se existir) ou faz downgrade
    /// para o plano indicado em DowngradeToPlanId.
    /// </summary>
    [Authorize(Policy = SystemPermissions.PlansManage)]
    public async Task<bool> TerminateTrialAsync(
        TerminateTrialInput input,
        [Service] ISender sender,
        [Service] ITenantService tenantService,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new TerminateTrialCommand(tenantService.TenantId, input.DowngradeToPlanId), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }

    /// <summary>
    /// Marca uma cobrança como paga. Se o tenant estiver suspenso, restaura para ativo.
    /// </summary>
    [Authorize(Policy = SystemPermissions.PlansManage)]
    public async Task<bool> MarkBillingAsPaidAsync(
        MarkBillingAsPaidInput input,
        [Service] ISender sender,
        [Service] ITenantService tenantService,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new MarkBillingAsPaidCommand(input.BillingId, tenantService.TenantId), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(result.Errors.Select(e =>
                ErrorBuilder.New().SetMessage(e).SetExtension("code", result.ErrorCode).Build()));
    }
}
