using MyCRM.CRM.Domain.Repositories;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Companies.DeleteCompany;

public sealed class DeleteCompanyHandler : IRequestHandler<DeleteCompanyCommand, Result>
{
    private readonly ICompanyRepository _repository;

    public DeleteCompanyHandler(ICompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteCompanyCommand request, CancellationToken ct)
    {
        var company = await _repository.GetByIdAsync(request.Id, ct);

        if (company is null)
            return Result.Failure("COMPANY_NOT_FOUND", "Company not found.");

        _repository.Delete(company);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
