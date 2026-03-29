using Microsoft.Extensions.Caching.Memory;
using MyCRM.Auth.Application.Services;

namespace MyCRM.Auth.Infrastructure.Services;

public sealed class MemoryCacheLoginAttemptTracker : ILoginAttemptTracker
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan EntryExpiration = LockoutDuration.Add(TimeSpan.FromMinutes(1));

    private readonly IMemoryCache _cache;

    public MemoryCacheLoginAttemptTracker(IMemoryCache cache) => _cache = cache;

    public bool IsLockedOut(string key)
    {
        if (!_cache.TryGetValue<LoginAttemptEntry>(key, out var entry)) return false;
        return entry!.LockedUntil > DateTimeOffset.UtcNow;
    }

    public void RecordFailure(string key)
    {
        var entry = _cache.GetOrCreate(key, e =>
        {
            e.AbsoluteExpirationRelativeToNow = EntryExpiration;
            return new LoginAttemptEntry();
        })!;

        entry.FailureCount++;

        if (entry.FailureCount >= MaxAttempts)
            entry.LockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);

        _cache.Set(key, entry, EntryExpiration);
    }

    public void ResetFailures(string key) => _cache.Remove(key);

    private sealed class LoginAttemptEntry
    {
        public int FailureCount { get; set; }
        public DateTimeOffset LockedUntil { get; set; }
    }
}
