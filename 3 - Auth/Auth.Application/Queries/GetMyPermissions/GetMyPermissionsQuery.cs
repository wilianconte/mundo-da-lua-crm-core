using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Queries.GetMyPermissions;

public record GetMyPermissionsQuery : IRequest<Result<IReadOnlyList<string>>>;
