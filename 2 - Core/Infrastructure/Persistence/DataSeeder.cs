using MyCRM.Domain.Entities;
using MyCRM.Shared.Kernel.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace MyCRM.Infrastructure.Persistence;

public static class DataSeeder
{
    public static readonly Guid SeedTenantId = new("00000000-0000-0000-0000-000000000001");

    public static async Task SeedAsync(AppDbContext db, ITenantService tenantService)
    {
        tenantService.SetTenant(SeedTenantId);

        if (await db.Customers.AnyAsync())
            return;

        var customers = new[]
        {
            Customer.Create(SeedTenantId, "Ana Souza", "ana.souza@email.com", CustomerType.Individual, "(11) 91234-5678", "123.456.789-00"),
            Customer.Create(SeedTenantId, "Tech Solutions Ltda", "contato@techsolutions.com.br", CustomerType.Company, "(21) 98765-4321", "12.345.678/0001-99"),
            Customer.Create(SeedTenantId, "Carlos Mendes", "carlos.mendes@email.com", CustomerType.Individual, "(31) 99876-5432", "987.654.321-00"),
            Customer.Create(SeedTenantId, "Farmácia Saúde", "farmacia@saude.com", CustomerType.Company, "(41) 93456-7890", "98.765.432/0001-11"),
            Customer.Create(SeedTenantId, "Beatriz Lima", "beatriz.lima@email.com", CustomerType.Individual, "(51) 94567-8901", "456.789.123-00"),
        };

        await db.Customers.AddRangeAsync(customers);
        await db.SaveChangesAsync();
    }
}
