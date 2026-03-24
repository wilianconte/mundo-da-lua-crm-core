using MyCRM.CRM.Application.DTOs;
using MediatR;
using MyCRM.Shared.Kernel.Results;

namespace MyCRM.CRM.Application.Commands.Employees.UpdateEmployee;

public record UpdateEmployeeCommand(
    Guid Id,
    string? EmployeeCode,
    DateOnly? HireDate,
    string? Position,
    string? Department,
    string? ContractType,
    string? WorkSchedule,
    decimal? WorkloadHours,
    string? PayrollNumber,
    Guid? ManagerEmployeeId,
    Guid? UnitId,
    string? CostCenter,
    string? Notes
) : IRequest<Result<EmployeeDto>>;
