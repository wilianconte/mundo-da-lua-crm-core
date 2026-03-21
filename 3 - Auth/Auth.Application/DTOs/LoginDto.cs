namespace MyCRM.Auth.Application.DTOs;

public record LoginDto(string Token, DateTimeOffset ExpiresAt, Guid UserId, string Name, string Email);
