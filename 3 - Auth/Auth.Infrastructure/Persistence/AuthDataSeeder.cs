using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.Auth.Infrastructure.Persistence;

public static class AuthDataSeeder
{
    public static readonly Guid SeedTenantId = new("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AuthDbContext db, ITenantService tenantService)
    {
        tenantService.SetTenant(SeedTenantId);

        var exists = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == "admin@mundodalua.com" && u.TenantId == SeedTenantId);

        if (exists) return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var user = User.Create(SeedTenantId, "Administrador", "admin@mundodalua.com", passwordHash);

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }
}
