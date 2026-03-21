using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record PersonDto(
    Guid Id,
    Guid TenantId,
    string FullName,
    string? PreferredName,
    string? DocumentNumber,
    DateOnly? BirthDate,
    Gender? Gender,
    MaritalStatus? MaritalStatus,
    string? Nationality,
    string? Occupation,
    string? Email,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WhatsAppNumber,
    string? ProfileImageUrl,
    PersonStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
