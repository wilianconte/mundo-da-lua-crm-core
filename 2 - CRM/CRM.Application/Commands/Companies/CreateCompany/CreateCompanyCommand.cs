using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Companies.CreateCompany;

public record CreateCompanyCommand(
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
) : IRequest<Result<CompanyDto>>;
