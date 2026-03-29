using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.Auth.Infrastructure.Persistence;

public static class AuthDataSeeder
{
    public static readonly Guid SeedTenantId = new("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Garante a existência do Role "Administrador", do usuário admin e do vínculo entre eles.
    /// O adminPersonId vem do CRM seed — quem orquestra é o MigrationExtensions do Gateway.
    /// </summary>
    public static async Task SeedAsync(AuthDbContext db, ITenantService tenantService, Guid? adminPersonId = null)
    {
        tenantService.SetTenant(SeedTenantId);

        var adminRole = await EnsureAdminRoleAsync(db);
        await EnsureAdminUserAsync(db, adminRole, adminPersonId);
    }

    // ── Role Administrador ────────────────────────────────────────────────────

    private static async Task<Role> EnsureAdminRoleAsync(AuthDbContext db)
    {
        var role = await db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Name == "Administrador" && r.TenantId == SeedTenantId);

        if (role is null)
        {
            role = Role.Create(
                SeedTenantId,
                name:        "Administrador",
                description: "Acesso total ao sistema — atribuído à conta administrativa padrão.");

            await db.Roles.AddAsync(role);
            await db.SaveChangesAsync();
        }

        return role;
    }

    // ── Usuário admin ─────────────────────────────────────────────────────────

    private static async Task EnsureAdminUserAsync(AuthDbContext db, Role adminRole, Guid? adminPersonId)
    {
        const string adminEmail = "admin@mundodalua.com";

        var user = await db.Users
            .Include(u => u.UserRoles)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == adminEmail && u.TenantId == SeedTenantId);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

        if (user is null)
        {
            user = User.Create(SeedTenantId, "Administrador", adminEmail, passwordHash, adminPersonId);
            await db.Users.AddAsync(user);
        }
        else
        {
            user.UpdatePassword(passwordHash);

            if (adminPersonId.HasValue)
                user.LinkToPerson(adminPersonId.Value);
        }

        await db.SaveChangesAsync();

        // Garante que o role Administrador está atribuído
        var alreadyHasRole = user.UserRoles.Any(ur => ur.RoleId == adminRole.Id);
        if (!alreadyHasRole)
        {
            var userRole = UserRole.Create(user.Id, adminRole.Id);
            await db.UserRoles.AddAsync(userRole);
            await db.SaveChangesAsync();
        }
    }
}
