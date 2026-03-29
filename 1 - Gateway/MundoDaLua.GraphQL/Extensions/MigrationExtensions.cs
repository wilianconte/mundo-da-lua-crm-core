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

        var tenantService   = sp.GetRequiredService<ITenantService>();

        // O CRM seed roda primeiro — garante que a Person do admin existe
        await DataSeeder.SeedAsync(customersDb, tenantService);

        // Recupera o PersonId da Person admin no CRM para vinculá-la ao User admin no Auth
        tenantService.SetTenant(DataSeeder.SeedTenantId);
        var adminPersonId = await GetAdminPersonIdAsync(customersDb);

        // O Auth seed usa o PersonId do admin para vincular User ↔ Person
        await AuthDataSeeder.SeedAsync(authDb, tenantService, adminPersonId);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna o Id da Person do administrador no CRM (busca por e-mail).
    /// Retorna null se a Person ainda não existir (seed do CRM não rodou).
    /// </summary>
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

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = '{schema}' AND table_name = '{table}')";
        var exists = (bool)(await cmd.ExecuteScalarAsync() ?? false);

        if (!exists)
            await db.Database.ExecuteSqlRawAsync($"DELETE FROM \"{schema}\".\"__EFMigrationsHistory\"");
    }
}
