using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Users.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPermissionService _permissionService;
    private readonly ITenantService _tenant;

    public UpdateUserHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IPermissionService permissionService,
        ITenantService tenant)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _permissionService = permissionService;
        _tenant = tenant;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.Id, ct);
        if (user is null || user.TenantId != _tenant.TenantId)
            return Result<UserDto>.Failure("USER_NOT_FOUND", "User not found.");

        var emailExists = await _userRepository.EmailExistsAsync(_tenant.TenantId, request.Email, request.Id, ct);
        if (emailExists)
            return Result<UserDto>.Failure("USER_EMAIL_DUPLICATE", "A user with this email already exists.");

        if (request.PersonId.HasValue)
        {
            var personAlreadyLinked = await _userRepository.PersonIdAlreadyLinkedAsync(_tenant.TenantId, request.PersonId.Value, request.Id, ct);
            if (personAlreadyLinked)
                return Result<UserDto>.Failure("USER_PERSON_ALREADY_LINKED", "This person is already linked to another user.");
        }

        var roleValidationResult = await ValidateRolesAsync(request.RoleIds, ct);
        if (roleValidationResult is not null)
            return roleValidationResult;

        user.UpdateProfile(request.Name, request.Email, request.PersonId, request.IsActive);

        if (request.IsAdmin) user.SetAdmin();
        else user.UnsetAdmin();

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.UpdatePassword(_passwordHasher.Hash(request.Password));

        if (request.RoleIds is not null)
            user.SyncRoles(request.RoleIds);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);
        _permissionService.InvalidateCache(user.Id);

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.TenantId,
            user.Name,
            user.Email,
            user.IsActive,
            user.IsAdmin,
            user.PersonId,
            user.CreatedAt,
            user.UpdatedAt,
            user.CreatedBy,
            user.UpdatedBy));
    }

    private async Task<Result<UserDto>?> ValidateRolesAsync(IReadOnlyList<Guid>? roleIds, CancellationToken ct)
    {
        if (roleIds is null)
            return null;

        var uniqueRoleIds = roleIds.Distinct().ToArray();
        if (uniqueRoleIds.Length == 0)
            return null;

        var tenantRoles = await _roleRepository.GetByIdsAsync(uniqueRoleIds, ct);
        if (tenantRoles.Count == uniqueRoleIds.Length)
            return null;

        var allFoundRoles = await _roleRepository.GetByIdsIgnoringQueryFiltersAsync(uniqueRoleIds, ct);
        if (allFoundRoles.Count != uniqueRoleIds.Length)
            return Result<UserDto>.Failure("ROLE_NOT_FOUND", "One or more roles were not found.");

        var mismatchedRoleIds = allFoundRoles
            .Where(r => r.TenantId != _tenant.TenantId)
            .Select(r => r.Id)
            .ToArray();

        return mismatchedRoleIds.Length > 0
            ? Result<UserDto>.Failure("ROLE_TENANT_MISMATCH", "One or more roles belong to a different tenant.")
            : Result<UserDto>.Failure("ROLE_NOT_FOUND", "One or more roles were not found.");
    }
}
