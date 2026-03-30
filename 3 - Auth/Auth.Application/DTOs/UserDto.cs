namespace MyCRM.Auth.Application.DTOs;

public record UserDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Email,
    bool IsActive,
    Guid? PersonId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy
);
