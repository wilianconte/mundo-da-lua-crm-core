using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.Repositories;

namespace MyCRM.Auth.Domain.Repositories;

public interface IPlanRepository : IRepository<Plan>
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Plan?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Plan>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Plan?> GetFreePlanAsync(CancellationToken ct = default);
    Task<PlanFeature?> GetPlanFeatureAsync(Guid planId, string featureKey, CancellationToken ct = default);
}
