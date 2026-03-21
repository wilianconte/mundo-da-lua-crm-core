using MyCRM.Shared.Kernel.MultiTenancy;

namespace MyCRM.GraphQL.MultiTenancy;

/// <summary>
/// Lê o tenant_id diretamente do claim JWT via IHttpContextAccessor.
/// Compatível com Hot Chocolate que cria um novo DI scope por resolver.
/// </summary>
public sealed class HttpTenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid _tenantId;

    public HttpTenantService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public Guid TenantId
    {
        get
        {
            // Requests HTTP autenticadas: lê claim do JWT
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("tenant_id");
            if (claim is not null && Guid.TryParse(claim.Value, out var fromClaim))
                return fromClaim;

            // Fallback para seed/background (sem HTTP context)
            if (_tenantId != Guid.Empty)
                return _tenantId;

            throw new InvalidOperationException("Tenant não definido para a requisição atual.");
        }
    }

    public void SetTenant(Guid tenantId) => _tenantId = tenantId;
}
