namespace MyCRM.GraphQL.GraphQL.Employees.Inputs;

public record CreateEmployeeInput(
    Guid PersonId,
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
);
