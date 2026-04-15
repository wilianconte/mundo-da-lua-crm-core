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
    /// Retorna o TenantPlan corrente do tenant autenticado, incluindo o plano e suas features.
    /// Inclui planos com Status = Active ou PendingCancellation — ambos representam o plano
    /// em vigor: Active é o estado normal; PendingCancellation significa que o cancelamento foi
    /// solicitado mas o plano ainda está válido até EndDate (permite exibir o botão "Reverter
    /// cancelamento" no frontend e não quebra a navegação pós-cancelamento).
    /// Retorna null apenas se o tenant não possuir nenhum plano em vigor (estado inválido em produção).
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
                     && (x.Status == TenantPlanStatus.Active
                      || x.Status == TenantPlanStatus.PendingCancellation));

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
    /// Retorna os dados básicos do tenant autenticado.
    /// Usado pelo frontend para detectar suspensão por inadimplência (Tenant.Status = Suspended).
    /// </summary>
    [Authorize]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Tenant> GetMyTenant(
        [Service] AuthDbContext db,
        [Service] ITenantService tenantService)
        => db.Tenants
            .AsNoTracking()
            .Where(x => x.Id == tenantService.TenantId);

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
