using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Employees.UpdateEmployee;

public sealed class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;
    private readonly ITenantService      _tenant;

    public UpdateEmployeeHandler(IEmployeeRepository repository, ITenantService tenant)
    {
        _repository = repository;
        _tenant     = tenant;
    }

    public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken ct)
    {
        var employee = await _repository.GetByIdAsync(request.Id, ct);
        if (employee is null)
            return Result<EmployeeDto>.Failure("EMPLOYEE_NOT_FOUND", "Employee not found.");

        // Prevent duplicate employee code (excluding the current record)
        if (request.EmployeeCode is not null)
        {
            var codeExists = await _repository.EmployeeCodeExistsAsync(
                _tenant.TenantId, request.EmployeeCode, excludeId: request.Id, ct: ct);
            if (codeExists)
                return Result<EmployeeDto>.Failure(
                    "EMPLOYEE_CODE_DUPLICATE",
                    "An employee with this code already exists.");
        }

        employee.UpdateInfo(
            employeeCode:      request.EmployeeCode,
            hireDate:          request.HireDate,
            position:          request.Position,
            department:        request.Department,
            contractType:      request.ContractType,
            workSchedule:      request.WorkSchedule,
            workloadHours:     request.WorkloadHours,
            payrollNumber:     request.PayrollNumber,
            managerEmployeeId: request.ManagerEmployeeId,
            unitId:            request.UnitId,
            costCenter:        request.CostCenter,
            notes:             request.Notes);

        _repository.Update(employee);
        await _repository.SaveChangesAsync(ct);

        return Result<EmployeeDto>.Success(employee.Adapt<EmployeeDto>());
    }
}
