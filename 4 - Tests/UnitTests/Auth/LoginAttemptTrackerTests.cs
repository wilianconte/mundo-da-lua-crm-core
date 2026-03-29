using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MyCRM.Auth.Infrastructure.Services;

namespace MyCRM.UnitTests.Auth;

public sealed class LoginAttemptTrackerTests : IDisposable
{
    private readonly IMemoryCache _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
    private readonly MemoryCacheLoginAttemptTracker _tracker;
    private const string Key = "login:tenant-id:user@test.com";

    public LoginAttemptTrackerTests() => _tracker = new MemoryCacheLoginAttemptTracker(_cache);

    [Fact]
    public void IsLockedOut_WithNoAttempts_ReturnsFalse()
    {
        Assert.False(_tracker.IsLockedOut(Key));
    }

    [Fact]
    public void IsLockedOut_AfterFourFailures_ReturnsFalse()
    {
        for (var i = 0; i < 4; i++)
            _tracker.RecordFailure(Key);

        Assert.False(_tracker.IsLockedOut(Key));
    }

    [Fact]
    public void IsLockedOut_AfterFiveFailures_ReturnsTrue()
    {
        for (var i = 0; i < 5; i++)
            _tracker.RecordFailure(Key);

        Assert.True(_tracker.IsLockedOut(Key));
    }

    [Fact]
    public void ResetFailures_AfterLockout_UnlocksAccount()
    {
        for (var i = 0; i < 5; i++)
            _tracker.RecordFailure(Key);

        _tracker.ResetFailures(Key);

        Assert.False(_tracker.IsLockedOut(Key));
    }

    [Fact]
    public void RecordFailure_DifferentKeys_DoNotInterfere()
    {
        const string otherKey = "login:tenant-id:other@test.com";

        for (var i = 0; i < 5; i++)
            _tracker.RecordFailure(Key);

        Assert.True(_tracker.IsLockedOut(Key));
        Assert.False(_tracker.IsLockedOut(otherKey));
    }

    public void Dispose() => _cache.Dispose();
}
