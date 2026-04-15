namespace MyCRM.Shared.Kernel.Services;

/// <summary>
/// Verifica se a ação solicitada respeita os limites do plano ativo do tenant.
/// Lança <see cref="MyCRM.Shared.Kernel.Exceptions.PlanLimitException"/> caso o limite seja atingido.
/// </summary>
public interface IPlanLimitService
{
    /// <summary>
    /// Verifica uma feature numérica (ex: max_students).
    /// Chama <paramref name="getCurrentCountAsync"/> para obter o valor atual
    /// e rejeita se count &gt;= Value (quando Value não é null).
    /// </summary>
    Task CheckNumericLimitAsync(Guid tenantId, string featureKey, Func<Task<int>> getCurrentCountAsync, CancellationToken ct = default);

    /// <summary>
    /// Verifica se uma feature booleana está habilitada no plano ativo.
    /// Rejeita se Value = 0.
    /// </summary>
    Task CheckBooleanFeatureAsync(Guid tenantId, string featureKey, CancellationToken ct = default);
}
