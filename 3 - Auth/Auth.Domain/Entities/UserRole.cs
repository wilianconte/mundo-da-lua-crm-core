using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

public class UserRole : TenantEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    private UserRole() { }

    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
    }
}
