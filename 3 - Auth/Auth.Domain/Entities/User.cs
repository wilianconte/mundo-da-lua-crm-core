using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

public class User : TenantEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public bool IsAdmin { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    /// <summary>
    /// Referência ao registro Person do módulo CRM.
    /// Cada usuário está vinculado a exatamente uma pessoa.
    /// Sem navigation property — módulos Auth e CRM são independentes.
    /// </summary>
    public Guid? PersonId { get; private set; }

    private User() { }

    public static User Create(Guid tenantId, string name, string email, string passwordHash, Guid? personId = null)
    {
        return new User
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsActive = true,
            PersonId = personId,
        };
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Touch();
    }

    public void UpdateProfile(string name, string email, Guid? personId, bool isActive)
    {
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PersonId = personId;
        IsActive = isActive;
        Touch();
    }

    public void SyncRoles(IReadOnlyCollection<Guid> roleIds)
    {
        var targetRoleIds = roleIds.Distinct().ToHashSet();
        _userRoles.RemoveAll(x => !targetRoleIds.Contains(x.RoleId));

        var existingRoleIds = _userRoles.Select(x => x.RoleId).ToHashSet();
        foreach (var roleId in targetRoleIds)
        {
            if (!existingRoleIds.Contains(roleId))
                _userRoles.Add(UserRole.Create(Id, roleId));
        }

        Touch();
    }

    public void LinkToPerson(Guid personId)
    {
        PersonId = personId;
        Touch();
    }

    public void UnlinkFromPerson()
    {
        PersonId = null;
        Touch();
    }

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public void SetPasswordResetToken(string token, DateTime expiresAt)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = expiresAt;
        Touch();
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        Touch();
    }

    public void ResetPassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        ClearPasswordResetToken();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }

    public void SetAdmin() { IsAdmin = true; Touch(); }
    public void UnsetAdmin() { IsAdmin = false; Touch(); }
}
