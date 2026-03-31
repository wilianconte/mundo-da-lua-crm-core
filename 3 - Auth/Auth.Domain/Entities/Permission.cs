using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name         { get; private set; } = default!;
    public string? Description { get; private set; }
    public string Group        { get; private set; } = default!;
    public bool IsActive       { get; private set; }

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { }

    public static Permission Create(string name, string group, string? description = null) =>
        new()
        {
            Name        = name.Trim(),
            Group       = group.Trim(),
            Description = description?.Trim(),
            IsActive    = true,
        };

    public void Reactivate()
    {
        IsActive = true;
        Touch();
    }
}
