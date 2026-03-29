using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

public class Role : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string[] Permissions { get; private set; } = Array.Empty<string>();
    public bool IsActive { get; private set; }

    private Role() { }

    public static Role Create(string name, string? description, string[]? permissions)
    {
        return new Role
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Permissions = permissions ?? Array.Empty<string>(),
            IsActive = true
        };
    }

    public void Update(string name, string? description, string[]? permissions)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Permissions = permissions ?? Array.Empty<string>();
        Touch();
    }

    public void UpdatePermissions(string[] permissions)
    {
        Permissions = permissions ?? Array.Empty<string>();
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}
