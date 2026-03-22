using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Companies.CreateCompany;

public sealed class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, Result<CompanyDto>>
{
    private readonly ICompanyRepository _repository;
    private readonly ITenantService _tenant;

    public CreateCompanyHandler(ICompanyRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<CompanyDto>> Handle(CreateCompanyCommand request, CancellationToken ct)
    {
        if (request.Email is not null)
        {
            var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, ct: ct);
            if (emailExists)
                return Result<CompanyDto>.Failure("COMPANY_EMAIL_DUPLICATE", "A company with this email already exists.");
        }

        if (request.RegistrationNumber is not null)
        {
            var regExists = await _repository.RegistrationNumberExistsAsync(_tenant.TenantId, request.RegistrationNumber, ct: ct);
            if (regExists)
                return Result<CompanyDto>.Failure("COMPANY_REGISTRATION_DUPLICATE", "A company with this registration number already exists.");
        }

        var company = Company.Create(
            tenantId:             _tenant.TenantId,
            legalName:            request.LegalName,
            tradeName:            request.TradeName,
            registrationNumber:   request.RegistrationNumber,
            stateRegistration:    request.StateRegistration,
            municipalRegistration: request.MunicipalRegistration,
            email:                request.Email,
            primaryPhone:         request.PrimaryPhone,
            secondaryPhone:       request.SecondaryPhone,
            whatsAppNumber:       request.WhatsAppNumber,
            website:              request.Website,
            contactPersonName:    request.ContactPersonName,
            contactPersonEmail:   request.ContactPersonEmail,
            contactPersonPhone:   request.ContactPersonPhone,
            companyType:          request.CompanyType,
            industry:             request.Industry,
            profileImageUrl:      request.ProfileImageUrl,
            notes:                request.Notes);

        await _repository.AddAsync(company, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<CompanyDto>.Success(company.Adapt<CompanyDto>());
    }
}
