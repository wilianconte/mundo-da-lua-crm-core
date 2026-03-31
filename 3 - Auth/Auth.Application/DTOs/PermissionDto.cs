namespace MyCRM.Auth.Application.DTOs;

public record PermissionDto(
    Guid    Id,
    string  Name,
    string  Group,
    string? Description,
    bool    IsActive);
