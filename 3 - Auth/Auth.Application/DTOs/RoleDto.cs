namespace MyCRM.Auth.Application.DTOs;

public record RoleDto(
    Guid    Id,
    Guid    TenantId,
    string  Name,
    string? Description,
    bool    IsActive,
    IReadOnlyList<PermissionDto> Permissions,
    DateTimeOffset  CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid?           CreatedBy,
    Guid?           UpdatedBy);
