using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Companies.UpdateCompany;

public sealed class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, Result<CompanyDto>>
{
    private readonly ICompanyRepository _repository;
    private readonly ITenantService _tenant;

    public UpdateCompanyHandler(ICompanyRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant = tenant;
    }

    public async Task<Result<CompanyDto>> Handle(UpdateCompanyCommand request, CancellationToken ct)
    {
        var company = await _repository.GetByIdAsync(request.Id, ct);

        if (company is null)
            return Result<CompanyDto>.Failure("COMPANY_NOT_FOUND", "Company not found.");

        if (request.Email is not null)
        {
            var emailExists = await _repository.EmailExistsAsync(_tenant.TenantId, request.Email, request.Id, ct);
            if (emailExists)
                return Result<CompanyDto>.Failure("COMPANY_EMAIL_DUPLICATE", "A company with this email already exists.");
        }

        if (request.RegistrationNumber is not null)
        {
            var regExists = await _repository.RegistrationNumberExistsAsync(_tenant.TenantId, request.RegistrationNumber, request.Id, ct);
            if (regExists)
                return Result<CompanyDto>.Failure("COMPANY_REGISTRATION_DUPLICATE", "A company with this registration number already exists.");
        }

        company.UpdateProfile(
            request.LegalName,
            request.TradeName,
            request.StateRegistration,
            request.MunicipalRegistration,
            request.CompanyType,
            request.Industry,
            request.ProfileImageUrl,
            request.Notes);

        company.UpdateContact(
            request.Email,
            request.PrimaryPhone,
            request.SecondaryPhone,
            request.WhatsAppNumber,
            request.Website,
            request.ContactPersonName,
            request.ContactPersonEmail,
            request.ContactPersonPhone);

        company.UpdateRegistrationNumber(request.RegistrationNumber);

        _repository.Update(company);
        await _repository.SaveChangesAsync(ct);

        return Result<CompanyDto>.Success(company.Adapt<CompanyDto>());
    }
}
