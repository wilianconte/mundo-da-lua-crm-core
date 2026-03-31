using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

public class RolePermission : BaseEntity
{
    public Guid RoleId       { get; private set; }
    public Guid PermissionId { get; private set; }

    public Role       Role       { get; private set; } = default!;
    public Permission Permission { get; private set; } = default!;

    private RolePermission() { }

    public static RolePermission Create(Guid roleId, Guid permissionId) =>
        new() { RoleId = roleId, PermissionId = permissionId };
}
