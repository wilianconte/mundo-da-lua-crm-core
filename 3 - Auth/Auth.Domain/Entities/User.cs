using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.Auth.Domain.Entities;

public class User : TenantEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; }

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

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
}
