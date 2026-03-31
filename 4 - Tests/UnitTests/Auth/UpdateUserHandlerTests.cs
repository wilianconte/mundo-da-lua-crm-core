using MyCRM.Auth.Application.Commands.Users.UpdateUser;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using NSubstitute;

namespace MyCRM.UnitTests.Auth;

public sealed class UpdateUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITenantService _tenant = Substitute.For<ITenantService>();
    private readonly UpdateUserHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdateUserHandlerTests()
    {
        _tenant.TenantId.Returns(_tenantId);
        _handler = new UpdateUserHandler(_userRepository, _roleRepository, _passwordHasher, _tenant);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        _userRepository.GetByIdWithRolesAsync(Arg.Any<Guid>(), default).Returns((User?)null);

        var result = await _handler.Handle(
            new UpdateUserCommand(Guid.NewGuid(), "Maria", "maria@test.com", null, true, null),
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        var user = User.Create(_tenantId, "Maria", "maria@test.com", "hash");
        _userRepository.GetByIdWithRolesAsync(user.Id, default).Returns(user);
        _userRepository.EmailExistsAsync(_tenantId, "other@test.com", user.Id, default).Returns(true);

        var result = await _handler.Handle(
            new UpdateUserCommand(user.Id, "Maria", "other@test.com", null, true, null),
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal("USER_EMAIL_DUPLICATE", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UpdateRoles_ReplacesAssociations()
    {
        var tenantRole = Role.Create(_tenantId, "Professor");

        var user = User.Create(_tenantId, "Maria", "maria@test.com", "hash");
        user.SyncRoles([Guid.NewGuid()]);

        _userRepository.GetByIdWithRolesAsync(user.Id, default).Returns(user);
        _userRepository.EmailExistsAsync(_tenantId, "maria.nova@test.com", user.Id, default).Returns(false);
        _roleRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default).Returns([tenantRole]);
        _passwordHasher.Hash("NovaSenha123!").Returns("new-hash");

        var result = await _handler.Handle(
            new UpdateUserCommand(
                user.Id,
                "Maria Nova",
                "maria.nova@test.com",
                Guid.NewGuid(),
                false,
                "NovaSenha123!",
                [tenantRole.Id]),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Maria Nova", result.Value!.Name);
        Assert.Equal("maria.nova@test.com", result.Value.Email);
        Assert.False(result.Value.IsActive);
        Assert.Single(user.UserRoles);
        Assert.Equal(tenantRole.Id, user.UserRoles.Single().RoleId);
    }

    [Fact]
    public async Task Handle_WithRoleFromDifferentTenant_ReturnsTenantMismatch()
    {
        var roleId = Guid.NewGuid();
        var user = User.Create(_tenantId, "Maria", "maria@test.com", "hash");

        _userRepository.GetByIdWithRolesAsync(user.Id, default).Returns(user);
        _userRepository.EmailExistsAsync(_tenantId, "maria@test.com", user.Id, default).Returns(false);
        _roleRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default).Returns([]);
        _roleRepository.GetByIdsIgnoringQueryFiltersAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default)
            .Returns([Role.Create(Guid.NewGuid(), "Professor")]);

        var result = await _handler.Handle(
            new UpdateUserCommand(user.Id, "Maria", "maria@test.com", null, true, null, [roleId]),
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_TENANT_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_WithRoleNotFound_ReturnsFailure()
    {
        var roleId = Guid.NewGuid();
        var user = User.Create(_tenantId, "Maria", "maria@test.com", "hash");

        _userRepository.GetByIdWithRolesAsync(user.Id, default).Returns(user);
        _userRepository.EmailExistsAsync(_tenantId, "maria@test.com", user.Id, default).Returns(false);
        _roleRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default).Returns([]);
        _roleRepository.GetByIdsIgnoringQueryFiltersAsync(Arg.Any<IReadOnlyCollection<Guid>>(), default).Returns([]);

        var result = await _handler.Handle(
            new UpdateUserCommand(user.Id, "Maria", "maria@test.com", null, true, null, [roleId]),
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal("ROLE_NOT_FOUND", result.ErrorCode);
    }
}
