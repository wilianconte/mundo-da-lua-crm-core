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

        await SeedRolesAsync(db);
        await SeedUsersAsync(db);
    }

    private static async Task SeedRolesAsync(AuthDbContext db)
    {
        if (await db.Roles.AnyAsync()) return;

        var roles = new[]
        {
            Role.Create("Admin", "Administrador com acesso total ao sistema", new[]
            {
                "users.read", "users.write", "users.delete",
                "roles.read", "roles.write", "roles.delete",
                "customers.read", "customers.write", "customers.delete",
                "students.read", "students.write", "students.delete",
                "courses.read", "courses.write", "courses.delete",
                "employees.read", "employees.write", "employees.delete"
            }),
            Role.Create("Manager", "Gerente com permissões de leitura e escrita", new[]
            {
                "users.read",
                "customers.read", "customers.write",
                "students.read", "students.write",
                "courses.read", "courses.write",
                "employees.read"
            }),
            Role.Create("User", "Usuário padrão com permissões básicas de leitura", new[]
            {
                "customers.read",
                "students.read",
                "courses.read"
            }),
            Role.Create("Viewer", "Visualizador com acesso somente leitura", new[]
            {
                "customers.read",
                "students.read",
                "courses.read",
                "employees.read"
            })
        };

        await db.Roles.AddRangeAsync(roles);
        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AuthDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var user = User.Create(SeedTenantId, "Administrador", "admin@mundodalua.com", passwordHash);
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        // Atribuir role Admin ao usuário admin
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is not null)
        {
            var userRole = UserRole.Create(user.Id, adminRole.Id);
            await db.UserRoles.AddAsync(userRole);
            await db.SaveChangesAsync();
        }
    }
}
