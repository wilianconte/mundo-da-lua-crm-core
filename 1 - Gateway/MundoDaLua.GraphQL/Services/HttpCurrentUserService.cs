using MyCRM.Shared.Kernel.Audit;
using System.Security.Claims;

namespace MyCRM.GraphQL.Services;

/// <summary>
/// Extracts the current user ID from JWT claims via IHttpContextAccessor.
/// Returns null for unauthenticated requests (e.g. login, seed, background jobs).
/// </summary>
public sealed class HttpCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
