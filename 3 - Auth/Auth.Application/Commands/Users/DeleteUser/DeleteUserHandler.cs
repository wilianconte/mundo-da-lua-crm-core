using MediatR;
using MyCRM.Auth.Application.Services;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Users.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _repository;
    private readonly IPermissionService _permissionService;
    private readonly ITenantService _tenant;

    public DeleteUserHandler(
        IUserRepository repository,
        IPermissionService permissionService,
        ITenantService tenant)
    {
        _repository = repository;
        _permissionService = permissionService;
        _tenant = tenant;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await _repository.GetByIdAsync(request.Id, ct);

        if (user is null || user.TenantId != _tenant.TenantId)
            return Result.Failure("USER_NOT_FOUND", "User not found.");

        _repository.Delete(user);
        await _repository.SaveChangesAsync(ct);
        _permissionService.InvalidateCache(user.Id);

        return Result.Success();
    }
}
