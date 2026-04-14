using MyCRM.Auth.Domain.Entities;

namespace MyCRM.Auth.Application.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    Guid CompanyId,
    Guid? OwnerPersonId,
    TenantStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
