using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Domain.Repositories;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Queries.GetPermissions;

public sealed class GetPermissionsHandler : IRequestHandler<GetPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private readonly IPermissionRepository _repository;

    public GetPermissionsHandler(IPermissionRepository repository)
        => _repository = repository;

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(GetPermissionsQuery request, CancellationToken ct)
    {
        var permissions = await _repository.GetAllAsync(ct);
        var dtos = permissions
            .Select(p => new PermissionDto(p.Id, p.Name, p.Group, p.Description, p.IsActive))
            .ToList();

        return Result<IReadOnlyList<PermissionDto>>.Success(dtos);
    }
}
