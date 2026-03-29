using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

/// <summary>
/// Perfil de acesso — agrupa permissões e é atribuído a usuários por tenant.
/// Exemplos: Administrador, Professor, Coordenador, Secretaria.
/// </summary>
public class Role : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private Role() { }

    public static Role Create(Guid tenantId, string name, string? description = null)
    {
        return new Role
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
        };
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate()   { IsActive = true;  Touch(); }

    public void Update(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Touch();
    }
}
