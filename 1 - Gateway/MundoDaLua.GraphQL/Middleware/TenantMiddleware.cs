using MyCRM.Shared.Kernel.MultiTenancy;
using System.Security.Claims;

namespace MyCRM.GraphQL.Middleware;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var claim = context.User.FindFirst("tenant_id");
        if (claim is not null && Guid.TryParse(claim.Value, out var tenantId))
            tenantService.SetTenant(tenantId);

        await _next(context);
    }
}
