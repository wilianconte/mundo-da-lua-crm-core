using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Valor de uma feature para um plano específico.
/// Para features Numeric: Value contém o limite (null = ilimitado).
/// Para features Boolean: Value = 1 habilitado, 0 desabilitado.
/// </summary>
public sealed class PlanFeature : BaseEntity
{
    public Guid PlanId { get; private set; }
    public Plan Plan { get; private set; } = default!;

    public Guid FeatureId { get; private set; }
    public Feature Feature { get; private set; } = default!;

    /// <summary>
    /// Null = ilimitado/sem restrição (apenas para Numeric).
    /// Boolean: 0 = desabilitado, 1 = habilitado.
    /// </summary>
    public int? Value { get; private set; }

    private PlanFeature() { }

    public static PlanFeature Create(Guid planId, Guid featureId, int? value)
    {
        return new PlanFeature
        {
            PlanId    = planId,
            FeatureId = featureId,
            Value     = value,
        };
    }
}
