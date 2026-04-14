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

        await customersDb.Database.MigrateAsync();

        await authDb.Database.MigrateAsync();

        var tenantService = sp.GetRequiredService<ITenantService>();

        // CRM seed first so the admin Person and Company exist.
        await DataSeeder.SeedAsync(customersDb, tenantService);

        // Fetch the admin PersonId and CompanyId from CRM to link to Auth.
        tenantService.SetTenant(DataSeeder.SeedTenantId);
        var adminPersonId  = await GetAdminPersonIdAsync(customersDb);
        var adminCompanyId = await GetSeedCompanyIdAsync(customersDb);

        // Sync system permissions before ensuring admin role permissions.
        await PermissionSeeder.SeedAsync(authDb);

        // Seed Tenant + auth data and guarantee admin role has all active permissions.
        await AuthDataSeeder.SeedAsync(authDb, tenantService, adminPersonId, adminCompanyId);
    }

    private static async Task<Guid?> GetAdminPersonIdAsync(CRMDbContext crmDb)
    {
        const string adminEmail = "admin@mundodalua.com";

        return await crmDb.People
            .IgnoreQueryFilters()
            .Where(p => p.Email == adminEmail)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync();
    }

    private static async Task<Guid?> GetSeedCompanyIdAsync(CRMDbContext crmDb)
    {
        const string seedCnpj = "12.345.678/0001-90"; // Mundo da Lua Educação Ltda

        return await crmDb.Companies
            .IgnoreQueryFilters()
            .Where(c => c.RegistrationNumber == seedCnpj)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync();
    }

}
