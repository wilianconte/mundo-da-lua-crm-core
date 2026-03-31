using Microsoft.Extensions.Caching.Memory;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Auth.Infrastructure.Services;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class PermissionServiceTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPermissionRepository _permissionRepo = Substitute.For<IPermissionRepository>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly PermissionService _service;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _roleId = Guid.NewGuid();

    public PermissionServiceTests()
        => _service = new PermissionService(_userRepo, _permissionRepo, _cache);

    [Fact]
    public async Task GetUserPermissions_CacheMiss_QueriesDbAndCachesResult()
    {
        var user = CreateUserWithRole(_userId, _roleId);
        _userRepo.GetByIdWithRolesAsync(_userId, default).Returns(user);
        _permissionRepo.GetPermissionNamesByRoleIdsAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(_roleId)), default)
            .Returns(["students:read", "courses:read"]);

        var result = await _service.GetUserPermissionsAsync(_userId);

        Assert.Contains("students:read", result);
        Assert.Contains("courses:read", result);
        await _permissionRepo.Received(1).GetPermissionNamesByRoleIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default);
    }

    [Fact]
    public async Task GetUserPermissions_CacheHit_DoesNotQueryDb()
    {
        var user = CreateUserWithRole(_userId, _roleId);
        _userRepo.GetByIdWithRolesAsync(_userId, default).Returns(user);
        _permissionRepo.GetPermissionNamesByRoleIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default)
            .Returns(["students:read"]);

        await _service.GetUserPermissionsAsync(_userId);
        await _service.GetUserPermissionsAsync(_userId);

        await _permissionRepo.Received(1).GetPermissionNamesByRoleIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default);
    }

    [Fact]
    public async Task HasPermission_PermissionPresent_ReturnsTrue()
    {
        var user = CreateUserWithRole(_userId, _roleId);
        _userRepo.GetByIdWithRolesAsync(_userId, default).Returns(user);
        _permissionRepo.GetPermissionNamesByRoleIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default)
            .Returns(["students:read"]);

        var result = await _service.HasPermissionAsync(_userId, "students:read");

        Assert.True(result);
    }

    [Fact]
    public async Task HasPermission_PermissionAbsent_ReturnsFalse()
    {
        var user = CreateUserWithRole(_userId, _roleId);
        _userRepo.GetByIdWithRolesAsync(_userId, default).Returns(user);
        _permissionRepo.GetPermissionNamesByRoleIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default)
            .Returns(["students:read"]);

        var result = await _service.HasPermissionAsync(_userId, "students:delete");

        Assert.False(result);
    }

    [Fact]
    public async Task InvalidateCache_AfterInvalidation_QueriesDbAgain()
    {
        var user = CreateUserWithRole(_userId, _roleId);
        _userRepo.GetByIdWithRolesAsync(_userId, default).Returns(user);
        _permissionRepo.GetPermissionNamesByRoleIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default)
            .Returns(["students:read"]);

        await _service.GetUserPermissionsAsync(_userId);
        _service.InvalidateCache(_userId);
        await _service.GetUserPermissionsAsync(_userId);

        await _permissionRepo.Received(2).GetPermissionNamesByRoleIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default);
    }

    [Fact]
    public async Task GetUserPermissions_UserWithNoRoles_ReturnsEmpty()
    {
        var user = CreateUserWithRole(_userId);
        _userRepo.GetByIdWithRolesAsync(_userId, default).Returns(user);
        _permissionRepo.GetPermissionNamesByRoleIdsAsync(Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 0), default)
            .Returns([]);

        var result = await _service.GetUserPermissionsAsync(_userId);

        Assert.Empty(result);
    }

    private static User CreateUserWithRole(Guid userId, params Guid[] roleIds)
    {
        var user = User.Create(Guid.NewGuid(), "Test User", "test@test.com", "hash");
        if (roleIds.Length > 0)
            user.SyncRoles(roleIds);
        return user;
    }
}
