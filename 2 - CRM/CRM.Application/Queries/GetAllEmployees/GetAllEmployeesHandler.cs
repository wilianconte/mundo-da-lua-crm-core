using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetAllEmployees;

public sealed class GetAllEmployeesHandler : IRequestHandler<GetAllEmployeesQuery, Result<IReadOnlyList<EmployeeDto>>>
{
    private readonly IEmployeeRepository _repository;

    public GetAllEmployeesHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<EmployeeDto>>> Handle(GetAllEmployeesQuery request, CancellationToken ct)
    {
        var employees = await _repository.GetAllAsync(ct);
        var dtos      = employees.Adapt<IReadOnlyList<EmployeeDto>>();
        return Result<IReadOnlyList<EmployeeDto>>.Success(dtos);
    }
}
