using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Tenants.DeleteTenant;

public record DeleteTenantCommand(Guid Id) : IRequest<Result>;
