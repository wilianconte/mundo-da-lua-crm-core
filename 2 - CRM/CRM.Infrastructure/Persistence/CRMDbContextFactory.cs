using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.CRM.Infrastructure.Persistence;

/// <summary>
/// Usado exclusivamente pelo EF Core CLI (dotnet ef migrations add/update).
/// Não é invocado em tempo de execução.
/// </summary>
public sealed class CRMDbContextFactory : IDesignTimeDbContextFactory<CRMDbContext>
{
    public CRMDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CRMDbContext>()
            .UseNpgsql("Host=localhost;Database=mycrm_crm;Username=postgres;Password=postgres")
            .Options;

        return new CRMDbContext(options, new DesignTimeTenantService(), new DesignTimeCurrentUserService());
    }

    private sealed class DesignTimeTenantService : ITenantService
    {
        public Guid TenantId => Guid.Empty;
        public void SetTenant(Guid tenantId) { }
    }

    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
    }
}
