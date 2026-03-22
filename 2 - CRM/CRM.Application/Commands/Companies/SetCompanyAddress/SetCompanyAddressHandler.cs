using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Companies.SetCompanyAddress;

public sealed class SetCompanyAddressHandler : IRequestHandler<SetCompanyAddressCommand, Result<CompanyDto>>
{
    private readonly ICompanyRepository _repository;

    public SetCompanyAddressHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CompanyDto>> Handle(SetCompanyAddressCommand request, CancellationToken ct)
    {
        var company = await _repository.GetByIdAsync(request.CompanyId, ct);

        if (company is null)
            return Result<CompanyDto>.Failure("COMPANY_NOT_FOUND", "Company not found.");

        var address = new Address
        {
            Street       = request.Street,
            Number       = request.Number,
            Complement   = request.Complement,
            Neighborhood = request.Neighborhood,
            City         = request.City,
            State        = request.State,
            ZipCode      = request.ZipCode,
            Country      = request.Country,
        };

        company.SetAddress(address);

        _repository.Update(company);
        await _repository.SaveChangesAsync(ct);

        return Result<CompanyDto>.Success(company.Adapt<CompanyDto>());
    }
}
