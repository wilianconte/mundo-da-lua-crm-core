using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Enums;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.Shared.Kernel;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.GraphQL.GraphQL.Plans;

[QueryType]
public sealed class TenantPlanQueries
{
    /// <summary>
    /// Retorna o TenantPlan ativo do tenant autenticado, incluindo o plano e suas features.
    /// Retorna null se o tenant não possuir plano ativo (estado inválido em produção).
    /// </summary>
    [Authorize]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<TenantPlan> GetMyActivePlan(
        [Service] AuthDbContext db,
        [Service] ITenantService tenantService)
        => db.TenantPlans
            .AsNoTracking()
            .Include(x => x.Plan)
                .ThenInclude(p => p.PlanFeatures)
                    .ThenInclude(pf => pf.Feature)
            .Where(x => x.TenantId == tenantService.TenantId
                     && x.Status == TenantPlanStatus.Active);

    /// <summary>
    /// Retorna todos os planos ativos da plataforma com suas features, ordenados por SortOrder.
    /// Usado pelo frontend para exibir as opções de upgrade/trial disponíveis.
    /// </summary>
    [Authorize]
    [UseProjection]
    [UseSorting]
    public IQueryable<Plan> GetPlans([Service] AuthDbContext db)
        => db.Plans
            .AsNoTracking()
            .Include(x => x.PlanFeatures)
                .ThenInclude(pf => pf.Feature)
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder);

    /// <summary>
    /// Retorna o histórico de cobranças do tenant autenticado, paginado.
    /// Requer a permissão plans:manage.
    /// </summary>
    [Authorize(Policy = SystemPermissions.PlansManage)]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Billing> GetMyBillings(
        [Service] AuthDbContext db,
        [Service] ITenantService tenantService)
        => db.Billings
            .AsNoTracking()
            .Include(x => x.TenantPlan)
                .ThenInclude(tp => tp.Plan)
            .Where(x => x.TenantId == tenantService.TenantId);
}
