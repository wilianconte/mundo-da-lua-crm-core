using MyCRM.CRM.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.CRM.Domain.Repositories;

public interface ICommissionRuleRepository : IRepository<CommissionRule>
{
    Task<CommissionRule?> GetEffectiveRuleAsync(Guid tenantId, Guid professionalId, Guid serviceId, CancellationToken ct = default);
}
