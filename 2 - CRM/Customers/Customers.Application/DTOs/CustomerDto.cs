using MyCRM.Customers.Domain.Entities;

namespace MyCRM.Customers.Application.DTOs;

public record CustomerDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Email,
    string? Phone,
    string? Document,
    CustomerType Type,
    CustomerStatus Status,
    AddressDto? Address,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record AddressDto(
    string Street,
    string? Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country
);
