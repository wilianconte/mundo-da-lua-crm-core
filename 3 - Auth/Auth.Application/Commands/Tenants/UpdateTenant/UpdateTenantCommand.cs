using MediatR;
using MyCRM.Auth.Application.DTOs;
using MyCRM.Auth.Domain.Entities;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.Auth.Application.Commands.Tenants.UpdateTenant;

public record UpdateTenantCommand(
    Guid Id,
    string Name,
    TenantPlan Plan,
    TenantStatus Status
) : IRequest<Result<TenantDto>>;
