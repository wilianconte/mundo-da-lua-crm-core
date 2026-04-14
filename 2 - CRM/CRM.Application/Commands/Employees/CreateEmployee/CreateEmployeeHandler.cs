using MyCRM.CRM.Application.DTOs;
using MyCRM.CRM.Domain.Entities;
using MyCRM.CRM.Domain.Repositories;
using Mapster;
using MediatR;
using MyCRM.Shared.Kernel.Exceptions;
using MyCRM.Shared.Kernel.MultiTenancy;
using MyCRM.Shared.Kernel.Results;
using MyCRM.Shared.Kernel.Services;

namespace MyCRM.CRM.Application.Commands.Employees.CreateEmployee;

public sealed class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;
    private readonly IPersonRepository   _personRepository;
    private readonly ITenantService      _tenant;
    private readonly IPlanLimitService   _planLimit;

    public CreateEmployeeHandler(
        IEmployeeRepository repository,
        IPersonRepository personRepository,
        ITenantService tenant,
        IPlanLimitService planLimit)
    {
        _repository       = repository;
        _personRepository = personRepository;
        _tenant           = tenant;
        _planLimit        = planLimit;
    }

    public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        // Verifica limite de funcionários do plano
        try
        {
            await _planLimit.CheckNumericLimitAsync(
                _tenant.TenantId,
                "max_employees",
                () => _repository.CountActiveByTenantAsync(_tenant.TenantId, ct),
                ct);
        }
        catch (PlanLimitException ex)
        {
            return Result<EmployeeDto>.Failure(ex.ErrorCode, ex.Message);
        }

        // Ensure the Person exists within this tenant
        var person = await _personRepository.GetByIdAsync(request.PersonId, ct);
        if (person is null)
            return Result<EmployeeDto>.Failure("PERSON_NOT_FOUND", "Person not found.");

        // Prevent duplicate employee records for the same person within the tenant
        var alreadyEmployed = await _repository.PersonAlreadyEmployedAsync(
            _tenant.TenantId, request.PersonId, ct: ct);
        if (alreadyEmployed)
            return Result<EmployeeDto>.Failure(
                "EMPLOYEE_DUPLICATE_PERSON",
                "This person is already registered as an employee.");

        // Prevent duplicate employee codes
        if (request.EmployeeCode is not null)
        {
            var codeExists = await _repository.EmployeeCodeExistsAsync(
                _tenant.TenantId, request.EmployeeCode, ct: ct);
            if (codeExists)
                return Result<EmployeeDto>.Failure(
                    "EMPLOYEE_CODE_DUPLICATE",
                    "An employee with this code already exists.");
        }

        var employee = Employee.Create(
            tenantId:          _tenant.TenantId,
            personId:          request.PersonId,
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

        await _repository.AddAsync(employee, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<EmployeeDto>.Success(employee.Adapt<EmployeeDto>());
    }
}
