using MyCRM.CRM.Domain.Entities;

namespace MyCRM.GraphQL.GraphQL.Companies.Inputs;

public record CreateCompanyInput(
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
    string? Notes
);
