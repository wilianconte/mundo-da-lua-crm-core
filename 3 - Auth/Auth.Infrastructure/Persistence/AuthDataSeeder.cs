using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.Auth.Infrastructure.Persistence;

public static class AuthDataSeeder
{
    public static readonly Guid SeedTenantId = new("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Garante a existência do Tenant seed, do Role "Administrador", do usuário admin e vínculos.
    /// adminPersonId e adminCompanyId vêm do CRM seed — orquestrado pelo MigrationExtensions.
    /// </summary>
    public static async Task SeedAsync(
        AuthDbContext db,
        ITenantService tenantService,
        Guid? adminPersonId = null,
        Guid? adminCompanyId = null)
    {
        tenantService.SetTenant(SeedTenantId);

        await EnsureSeedTenantAsync(db, adminCompanyId, adminPersonId);

        var adminRole = await EnsureAdminRoleAsync(db);
        await EnsureAdminRoleHasAllPermissionsAsync(db, adminRole);
        await EnsureAdminUserAsync(db, adminRole, adminPersonId);
    }

    // ── Tenant Seed ───────────────────────────────────────────────────────────

    private static async Task EnsureSeedTenantAsync(AuthDbContext db, Guid? companyId, Guid? personId)
    {
        var exists = await db.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id == SeedTenantId);

        if (exists)
            return;

        var tenant = Tenant.Create(
            name:          "Mundo da Lua",
            companyId:     companyId ?? Guid.Empty,
            ownerPersonId: personId,
            id:            SeedTenantId);

        tenant.Activate();

        await db.Tenants.AddAsync(tenant);
        await db.SaveChangesAsync();
    }

    // ── Role Administrador ────────────────────────────────────────────────────

    private static async Task<Role> EnsureAdminRoleAsync(AuthDbContext db)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
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

    private static async Task EnsureAdminRoleHasAllPermissionsAsync(AuthDbContext db, Role adminRole)
    {
        var permissionIds = await db.Permissions
            .IgnoreQueryFilters()
            .Where(p => p.IsActive)
            .Select(p => p.Id)
            .ToListAsync();

        adminRole.SyncPermissions(permissionIds);
        db.Roles.Update(adminRole);
        await db.SaveChangesAsync();
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

        user.SetAdmin();

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
