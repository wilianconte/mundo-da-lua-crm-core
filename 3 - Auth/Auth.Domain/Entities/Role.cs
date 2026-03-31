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

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

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

    public void SyncPermissions(IReadOnlyCollection<Guid> permissionIds)
    {
        var target = permissionIds.Distinct().ToHashSet();
        _rolePermissions.RemoveAll(x => !target.Contains(x.PermissionId));

        var existing = _rolePermissions.Select(x => x.PermissionId).ToHashSet();
        foreach (var pid in target)
            if (!existing.Contains(pid))
                _rolePermissions.Add(RolePermission.Create(Id, pid));

        Touch();
    }
}
