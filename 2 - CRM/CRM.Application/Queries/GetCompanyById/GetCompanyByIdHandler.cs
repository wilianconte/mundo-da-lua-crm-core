using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetCompanyById;

public sealed class GetCompanyByIdHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDto>>
{
    private readonly ICompanyRepository _repository;

    public GetCompanyByIdHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CompanyDto>> Handle(GetCompanyByIdQuery request, CancellationToken ct)
    {
        var company = await _repository.GetByIdAsync(request.Id, ct);

        if (company is null)
            return Result<CompanyDto>.Failure("COMPANY_NOT_FOUND", "Company not found.");

        return Result<CompanyDto>.Success(company.Adapt<CompanyDto>());
    }
}
