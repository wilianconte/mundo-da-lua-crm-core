using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyCRM.Shared.Kernel.Audit;
using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.Auth.Infrastructure.Persistence;

public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        
        // Connection string para design time - ajustar conforme necessário
        optionsBuilder.UseNpgsql("Host=localhost;Database=mycrm_dev;Username=postgres;Password=postgres");
        
        // Serviços fake para design time
        var tenantService = new DesignTimeTenantService();
        var currentUserService = new DesignTimeCurrentUserService();
        
        return new AuthDbContext(optionsBuilder.Options, tenantService, currentUserService);
    }
}

// Implementações mínimas para design time
internal class DesignTimeTenantService : ITenantService
{
    public Guid TenantId { get; } = Guid.Empty;
    public void SetTenant(Guid tenantId) { }
}

internal class DesignTimeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; } = null;
}
