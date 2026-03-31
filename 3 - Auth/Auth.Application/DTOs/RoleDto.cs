namespace MyCRM.Auth.Application.DTOs;

public record RoleDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy);
