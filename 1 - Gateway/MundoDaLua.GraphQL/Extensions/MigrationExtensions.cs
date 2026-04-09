using MyCRM.CRM.Infrastructure.Persistence;
using MyCRM.CRM.Domain.Entities;
using MyCRM.Auth.Infrastructure.Persistence;
using MyCRM.Shared.Kernel.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.GraphQL.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateAllDbContextsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var customersDb = sp.GetRequiredService<CRMDbContext>();
        var authDb      = sp.GetRequiredService<AuthDbContext>();

        await ResetMigrationsIfSchemaLostAsync(customersDb, "crm", "customers");
        await customersDb.Database.MigrateAsync();

        await ResetMigrationsIfSchemaLostAsync(authDb, "auth", "users");
        await authDb.Database.MigrateAsync();

        var tenantService = sp.GetRequiredService<ITenantService>();

        // CRM seed first so the admin Person exists.
        await DataSeeder.SeedAsync(customersDb, tenantService);

        // Fetch the admin PersonId in CRM to link it to admin User in Auth.
        tenantService.SetTenant(DataSeeder.SeedTenantId);
        var adminPersonId = await GetAdminPersonIdAsync(customersDb);

        // Sync system permissions before ensuring admin role permissions.
        await PermissionSeeder.SeedAsync(authDb);

        // Seed auth data and guarantee admin role has all active permissions.
        await AuthDataSeeder.SeedAsync(authDb, tenantService, adminPersonId);
    }

    private static async Task<Guid?> GetAdminPersonIdAsync(CRMDbContext crmDb)
    {
        const string adminEmail = "admin@mundodalua.com";

        var person = await crmDb.People
            .IgnoreQueryFilters()
            .Where(p => p.Email == adminEmail)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync();

        return person;
    }

    private static async Task ResetMigrationsIfSchemaLostAsync(DbContext db, string schema, string table)
    {
        await db.Database.OpenConnectionAsync();
        var connection = db.Database.GetDbConnection();

        await using var checkTable = connection.CreateCommand();
        checkTable.CommandText = $"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = '{schema}' AND table_name = '{table}')";
        var tableExists = (bool)(await checkTable.ExecuteScalarAsync() ?? false);

        if (tableExists)
            return;

        await using var checkHistory = connection.CreateCommand();
        checkHistory.CommandText = $"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = '{schema}' AND table_name = '__EFMigrationsHistory')";
        var historyExists = (bool)(await checkHistory.ExecuteScalarAsync() ?? false);

        if (historyExists)
            await db.Database.ExecuteSqlRawAsync($"DELETE FROM \"{schema}\".\"__EFMigrationsHistory\"");
    }
}
