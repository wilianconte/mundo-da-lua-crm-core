using MyCRM.Shared.Kernel.MultiTenancy;
using Serilog.Context;
using System.Security.Claims;

namespace MyCRM.GraphQL.Middleware;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var tenantClaim = context.User.FindFirst("tenant_id");
        if (tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            tenantService.SetTenant(tenantId);

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub");

        using (LogContext.PushProperty("TenantId", tenantClaim?.Value ?? "anonymous"))
        using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
        {
            await _next(context);
        }
    }
}
