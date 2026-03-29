using MediatR;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Roles.DeleteRole;

public sealed class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleRepository _roleRepository;

    public DeleteRoleHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, ct);
        if (role is null)
            return Result.Failure("ROLE_NOT_FOUND", "Role not found.");

        _roleRepository.Delete(role);
        await _roleRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
