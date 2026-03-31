using Microsoft.EntityFrameworkCore;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel;

namespace MyCRM.Auth.Infrastructure.Persistence;

public static class PermissionSeeder
{
    public static async Task SeedAsync(AuthDbContext db)
    {
        foreach (var (name, group) in SystemPermissions.All)
        {
            var existing = await db.Permissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Name == name);

            if (existing is null)
            {
                var permission = Permission.Create(name, group);
                await db.Permissions.AddAsync(permission);
            }
            else if (!existing.IsActive)
            {
                existing.Reactivate();
                db.Permissions.Update(existing);
            }
        }

        await db.SaveChangesAsync();
    }
}
