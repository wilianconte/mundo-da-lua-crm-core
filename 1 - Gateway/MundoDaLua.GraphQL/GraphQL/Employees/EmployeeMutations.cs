using MediatR;
using MyCRM.CRM.Application.Commands.Employees.CreateEmployee;
using MyCRM.CRM.Application.Commands.Employees.UpdateEmployee;
using MyCRM.CRM.Application.Commands.Employees.DeleteEmployee;
using MyCRM.GraphQL.GraphQL.Employees.Inputs;

namespace MyCRM.GraphQL.GraphQL.Employees;

[Authorize]
[MutationType]
public sealed class EmployeeMutations
{
    public async Task<EmployeePayload> CreateEmployeeAsync(
        CreateEmployeeInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateEmployeeCommand(
            input.PersonId,
            input.EmployeeCode,
            input.HireDate,
            input.Position,
            input.Department,
            input.ContractType,
            input.WorkSchedule,
            input.WorkloadHours,
            input.PayrollNumber,
            input.ManagerEmployeeId,
            input.UnitId,
            input.CostCenter,
            input.Notes), ct);

        return result.IsSuccess
            ? new EmployeePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<EmployeePayload> UpdateEmployeeAsync(
        Guid id,
        UpdateEmployeeInput input,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateEmployeeCommand(
            id,
            input.EmployeeCode,
            input.HireDate,
            input.Position,
            input.Department,
            input.ContractType,
            input.WorkSchedule,
            input.WorkloadHours,
            input.PayrollNumber,
            input.ManagerEmployeeId,
            input.UnitId,
            input.CostCenter,
            input.Notes), ct);

        return result.IsSuccess
            ? new EmployeePayload(result.Value!)
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }

    public async Task<bool> DeleteEmployeeAsync(
        Guid id,
        [Service] ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteEmployeeCommand(id), ct);

        return result.IsSuccess
            ? true
            : throw new GraphQLException(
                result.Errors.Select(e =>
                    ErrorBuilder.New()
                        .SetMessage(e)
                        .SetExtension("code", result.ErrorCode)
                        .Build()));
    }
}
