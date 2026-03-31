using MyCRM.Auth.Domain.Entities;

namespace MyCRM.UnitTests.Auth;

public sealed class RoleSyncPermissionsTests
{
    [Fact]
    public void SyncPermissions_AddNewPermissions_AddsCorrectly()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin");
        var permId1 = Guid.NewGuid();
        var permId2 = Guid.NewGuid();

        role.SyncPermissions([permId1, permId2]);

        Assert.Equal(2, role.RolePermissions.Count);
        Assert.Contains(role.RolePermissions, rp => rp.PermissionId == permId1);
        Assert.Contains(role.RolePermissions, rp => rp.PermissionId == permId2);
    }

    [Fact]
    public void SyncPermissions_RemovePermissions_RemovesCorrectly()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin");
        var permId1 = Guid.NewGuid();
        var permId2 = Guid.NewGuid();
        role.SyncPermissions([permId1, permId2]);

        role.SyncPermissions([permId1]);

        Assert.Single(role.RolePermissions);
        Assert.Contains(role.RolePermissions, rp => rp.PermissionId == permId1);
    }

    [Fact]
    public void SyncPermissions_IsIdempotent_DoesNotDuplicate()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin");
        var permId = Guid.NewGuid();

        role.SyncPermissions([permId]);
        role.SyncPermissions([permId]);

        Assert.Single(role.RolePermissions);
    }

    [Fact]
    public void SyncPermissions_EmptyList_ClearsAllPermissions()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin");
        role.SyncPermissions([Guid.NewGuid(), Guid.NewGuid()]);

        role.SyncPermissions([]);

        Assert.Empty(role.RolePermissions);
    }

    [Fact]
    public void SyncPermissions_DuplicateIds_DeduplicatesCorrectly()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin");
        var permId = Guid.NewGuid();

        role.SyncPermissions([permId, permId]);

        Assert.Single(role.RolePermissions);
    }

    [Fact]
    public void SyncPermissions_TouchesUpdatedAt()
    {
        var role = Role.Create(Guid.NewGuid(), "Admin");
        var before = role.UpdatedAt;

        role.SyncPermissions([Guid.NewGuid()]);

        Assert.NotEqual(before, role.UpdatedAt);
    }
}
