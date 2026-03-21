using MyCRM.CRM.Infrastructure.Persistence;
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
        var authDb = sp.GetRequiredService<AuthDbContext>();

        await ResetMigrationsIfSchemaLostAsync(customersDb, "crm", "customers");
        await customersDb.Database.MigrateAsync();

        await authDb.Database.MigrateAsync();

        var tenantService = sp.GetRequiredService<ITenantService>();
        await DataSeeder.SeedAsync(customersDb, tenantService);
        await AuthDataSeeder.SeedAsync(authDb, tenantService);
    }

    private static async Task ResetMigrationsIfSchemaLostAsync(DbContext db, string schema, string table)
    {
        await db.Database.OpenConnectionAsync();
        var connection = db.Database.GetDbConnection();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = '{schema}' AND table_name = '{table}')";
        var exists = (bool)(await cmd.ExecuteScalarAsync() ?? false);

        if (!exists)
            await db.Database.ExecuteSqlRawAsync("DELETE FROM \"__EFMigrationsHistory\"");
    }
}
