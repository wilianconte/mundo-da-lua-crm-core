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

        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "admin@mundodalua.com" && u.TenantId == SeedTenantId);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

        if (user is null)
        {
            user = User.Create(SeedTenantId, "Administrador", "admin@mundodalua.com", passwordHash);
            await db.Users.AddAsync(user);
        }
        else
        {
            user.UpdatePassword(passwordHash);
        }

        await db.SaveChangesAsync();
    }
}
