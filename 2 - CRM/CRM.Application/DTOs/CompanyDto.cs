using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record CompanyDto(
    Guid Id,
    Guid TenantId,
    string LegalName,
    string? TradeName,
    string? RegistrationNumber,
    string? StateRegistration,
    string? MunicipalRegistration,
    string? Email,
    string? PrimaryPhone,
    string? SecondaryPhone,
    string? WhatsAppNumber,
    string? Website,
    string? ContactPersonName,
    string? ContactPersonEmail,
    string? ContactPersonPhone,
    CompanyType? CompanyType,
    string? Industry,
    string? ProfileImageUrl,
    AddressDto? Address,
    CompanyStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
