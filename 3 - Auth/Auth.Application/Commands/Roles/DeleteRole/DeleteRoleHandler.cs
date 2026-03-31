using MediatR;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.DeleteRole;

public sealed class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleRepository _repository;
    private readonly ITenantService _tenant;

    public DeleteRoleHandler(IRoleRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await _repository.GetByIdAsync(request.Id, ct);

        if (role is null || role.TenantId != _tenant.TenantId)
            return Result.Failure("ROLE_NOT_FOUND", "Role not found.");

        _repository.Delete(role);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
