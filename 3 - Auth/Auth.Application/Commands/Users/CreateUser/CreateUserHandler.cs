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
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantService _tenant;

    public CreateUserHandler(
        IUserRepository repository,
        IPasswordHasher passwordHasher,
        ITenantService tenant)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tenant = tenant;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, ct);
        if (emailExists)
            return Result<UserDto>.Failure("USER_EMAIL_DUPLICATE", "A user with this email already exists.");

        if (request.PersonId.HasValue)
        {
            var alreadyLinked = await _repository.PersonIdAlreadyLinkedAsync(_tenant.TenantId, request.PersonId.Value, ct);
            if (alreadyLinked)
                return Result<UserDto>.Failure("USER_PERSON_ALREADY_LINKED", "This person is already linked to another user.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(
            tenantId: _tenant.TenantId,
            name: request.Name,
            email: request.Email,
            passwordHash: passwordHash,
            personId: request.PersonId);

        await _repository.AddAsync(user, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.TenantId,
            user.Name,
            user.Email,
            user.IsActive,
            user.PersonId,
            user.CreatedAt,
            user.UpdatedAt,
            user.CreatedBy,
            user.UpdatedBy));
    }
}
