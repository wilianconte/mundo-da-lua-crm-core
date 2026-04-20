using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Users.CreateUser;

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _repository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantService _tenant;

    public CreateUserHandler(
        IUserRepository repository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ITenantService tenant)
    {
        _repository = repository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _tenant = tenant;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, ct: ct);
        if (emailExists)
            return Result<UserDto>.Failure("USER_EMAIL_DUPLICATE", "A user with this email already exists.");

        if (request.PersonId.HasValue)
        {
            var alreadyLinked = await _repository.PersonIdAlreadyLinkedAsync(_tenant.TenantId, request.PersonId.Value, ct: ct);
            if (alreadyLinked)
                return Result<UserDto>.Failure("USER_PERSON_ALREADY_LINKED", "This person is already linked to another user.");
        }

        var roleValidationResult = await ValidateRolesAsync(request.RoleIds, ct);
        if (roleValidationResult is not null)
            return roleValidationResult;

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(
            tenantId: _tenant.TenantId,
            name: request.Name,
            email: request.Email,
            passwordHash: passwordHash,
            personId: request.PersonId);

        if (request.IsAdmin)
            user.SetAdmin();

        if (request.RoleIds is not null)
            user.SyncRoles(request.RoleIds);

        await _repository.AddAsync(user, ct);
        await _repository.SaveChangesAsync(ct);

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
        if (roleIds is null || roleIds.Count == 0)
            return null;

        var uniqueRoleIds = roleIds.Distinct().ToArray();
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
