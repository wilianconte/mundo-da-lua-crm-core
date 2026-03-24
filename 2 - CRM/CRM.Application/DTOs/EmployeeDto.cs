using MyCRM.CRM.Domain.Entities;

namespace MyCRM.CRM.Application.DTOs;

public record EmployeeDto(
    Guid Id,
    Guid TenantId,
    Guid PersonId,
    string? EmployeeCode,
    DateOnly? HireDate,
    DateOnly? TerminationDate,
    string? Position,
    string? Department,
    string? ContractType,
    string? WorkSchedule,
    decimal? WorkloadHours,
    string? PayrollNumber,
    Guid? ManagerEmployeeId,
    Guid? UnitId,
    string? CostCenter,
    EmployeeStatus Status,
    bool IsActive,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
