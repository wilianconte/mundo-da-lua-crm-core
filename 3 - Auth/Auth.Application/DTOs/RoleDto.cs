namespace MyCRM.Auth.Application.DTOs;

public record RoleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public string[] Permissions { get; init; } = Array.Empty<string>();
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
