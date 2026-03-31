using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Queries.GetPermissions;

public record GetPermissionsQuery : IRequest<Result<IReadOnlyList<PermissionDto>>>;
