using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Tenant representa uma conta cliente isolada da plataforma MyCRM.
///
/// O Id desta entidade É o TenantId usado em todo o sistema — toda entidade
/// tenant-aware armazena este Guid em sua coluna TenantId.
///
/// Herda de BaseEntity (não TenantEntity) porque é a própria raiz do tenant;
/// não faz sentido filtrar tenants por TenantId.
///
/// Relacionamentos lógicos (sem FK física entre schemas):
///   CompanyId     → crm.companies  (a empresa proprietária da conta)
///   OwnerPersonId → crm.people     (a pessoa que registrou a conta)
///
/// O plano vigente é obtido via TenantPlan com Status = Active.
/// </summary>
public sealed class Tenant : BaseEntity
{
    /// <summary>Nome de exibição do tenant (geralmente o nome da empresa).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Referência lógica à empresa dona da conta (crm.companies).
    /// Sem FK física — schemas independentes.
    /// </summary>
    public Guid CompanyId { get; private set; }

    /// <summary>
    /// Referência lógica à pessoa que registrou/administra a conta (crm.people).
    /// Sem FK física — schemas independentes.
    /// </summary>
    public Guid? OwnerPersonId { get; private set; }

    public TenantStatus Status { get; private set; }

    private Tenant() { }

    /// <summary>
    /// Factory method — única forma de criar um novo Tenant.
    /// O Id gerado aqui é o TenantId que será propagado para todas as entidades.
    /// Passe <paramref name="id"/> explicitamente quando o TenantId precisar ser
    /// pré-determinado (ex.: registro de tenant onde o Id é gerado antes de criar
    /// User e Company para que o TenantId seja consistente desde o início).
    /// </summary>
    public static Tenant Create(
        string name,
        Guid companyId,
        Guid? ownerPersonId = null,
        Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Tenant
        {
            Id            = id ?? Guid.NewGuid(),
            Name          = name.Trim(),
            CompanyId     = companyId,
            OwnerPersonId = ownerPersonId,
            Status        = TenantStatus.Active,
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void Activate()  { Status = TenantStatus.Active;    Touch(); }
    public void Suspend()   { Status = TenantStatus.Suspended; Touch(); }
    public void Cancel()    { Status = TenantStatus.Cancelled; Touch(); }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Touch();
    }

    public void SetOwnerPerson(Guid personId)
    {
        OwnerPersonId = personId;
        Touch();
    }
}
