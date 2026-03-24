using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Queries.GetEmployeeById;

public sealed class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeByIdHandler(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken ct)
    {
        var employee = await _repository.GetByIdAsync(request.Id, ct);

        if (employee is null)
            return Result<EmployeeDto>.Failure("EMPLOYEE_NOT_FOUND", "Employee not found.");

        return Result<EmployeeDto>.Success(employee.Adapt<EmployeeDto>());
    }
}
